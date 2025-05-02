using System.Text;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using JiebaNet.Segmenter;
using NMeCab;

namespace NGramm
{
    public class TokenizerUtils
    {
        private static readonly Regex _startSpaces = new Regex(@"^\s+", RegexOptions.Compiled | RegexOptions.Multiline);
        private static readonly Regex _endSpaces = new Regex(@"\s+$", RegexOptions.Compiled | RegexOptions.Multiline);
        public static readonly Regex SpacesInRow = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.Multiline);
        
        private static readonly char[] _skipSpaces = { '\u3000' };
        
        private static List<UnicodeCategory> nonRenderingCategories = new List<UnicodeCategory> {
            UnicodeCategory.Control,
            UnicodeCategory.OtherNotAssigned,
            UnicodeCategory.Surrogate,
            UnicodeCategory.Format
        };
        
        #region CodeTextTokenize

        public static string RemoveStrings(string code, CommentDelimiters delimiters, out List<string> strings, bool removeStrings = true)
        {
            var result = new StringBuilder();
            int i = 0;
            int n = code.Length;
            strings = new List<string>();

            string activeMultiLineEnd = null;
            bool inSingleLineComment = false;
            bool inMultiLineComment = false;
            bool inString = false;
            char stringChar = '\0'; // Either ' or "
            bool escape = false;
            string temp = "";
            while (i < n)
            {
                char ch = code[i];
                if (!inString)
                {
                    if (inSingleLineComment)
                    {
                        result.Append(ch);
                        if (ch == '\n')
                        {
                            inSingleLineComment = false;
                        }
                        i++;
                        continue;
                    }
                    else
                    {
                        var matchedSingle = delimiters.SingleLine
                            .FirstOrDefault(sym => i + sym.Length <= n && code.Substring(i, sym.Length) == sym);
                        if (matchedSingle != null)
                        {
                            inSingleLineComment = true;
                            result.Append(matchedSingle);
                            i += matchedSingle.Length;
                            continue;
                        }
                    }

                    if (inMultiLineComment)
                    {
                        result.Append(ch);
                        if (i + activeMultiLineEnd.Length <= n && code.Substring(i, activeMultiLineEnd.Length) == activeMultiLineEnd)
                        {
                            result.Append(activeMultiLineEnd[1]);
                            i += activeMultiLineEnd.Length;
                            inMultiLineComment = false;
                            continue;
                        }
                        i++;
                        continue;
                    }
                    else
                    {
                        var matchedMulti = delimiters.MultiLine
                            .FirstOrDefault(pair => i + pair.Item1.Length <= n && code.Substring(i, pair.Item1.Length) == pair.Item1);
                        if (matchedMulti != null)
                        {
                            inMultiLineComment = true;
                            activeMultiLineEnd = matchedMulti.Item2;
                            result.Append(matchedMulti.Item1);
                            i += matchedMulti.Item1.Length;
                            continue;
                        }
                    }
                }


                if (inString)
                {
                    if (escape)
                    {
                        if (!removeStrings) temp+=code[i];
                        escape = false;
                        i++;
                    }
                    else if (ch == '\\')
                    {
                        escape = true;
                        i++;
                    }
                    else if (ch == stringChar)
                    {
                        inString = false;
                        result.Append(' ');
                        result.Append(stringChar); // Append closing quote
                        strings.Add(temp);
                        temp = "";
                        i++;
                    }
                    else
                    {
                        if (removeStrings) i++; 
                        else
                        {
                            temp+=code[i];
                            i++;
                        }
                    }
                    continue;
                }
                else if (ch == '\'' || ch == '\"')
                {
                    inString = true;
                    stringChar = ch;
                    result.Append(ch); // Append opening quote
                    i++;
                    continue;
                }
                result.Append(ch);
                i++;
            }
            
            return result.ToString();
        }
        public static string RemoveComments(string code, CommentDelimiters delimiters, out List<string> comments, bool removeComments = true)
        {
            var result = new StringBuilder();
            int i = 0;
            int n = code.Length;
            comments = new List<string>();

            char? inString = null;
            bool escape = false;

            while (i < n)
            {
                char ch = code[i];

                // String parsing logic
                if (inString != null)
                {
                    result.Append(ch);

                    if (escape)
                    {
                        escape = false;
                    }
                    else if (ch == '\\')
                    {
                        escape = true;
                    }
                    else if (ch == inString)
                    {
                        inString = null;  // end of string
                    }

                    i++;
                    continue;
                }
                else
                {
                    if (ch == '"' || ch == '\'')
                    {
                        inString = ch;
                        result.Append(ch);
                        i++;
                        continue;
                    }
                }

                    // Single-line
                    var matchedSingle = delimiters.SingleLine
                        .Where(sym => i + sym.Length <= n && code.Substring(i, sym.Length) == sym)
                        .OrderByDescending(sym => sym.Length)
                        .FirstOrDefault();

                    if (matchedSingle != null)
                    {
                        result.Append(matchedSingle);
                        i += matchedSingle.Length;
                        
                        if (removeComments)
                        {   
                            while (i < n && code[i] != '\n')
                            {
                                i++;
                            }
                        }
                        else
                        {
                            string temp = "";
                            while (i < n && code[i] != '\n')
                            {
                                temp+=code[i];
                                i++;
                            }
                            comments.Add(temp);
                        }
                        continue;
                    }

                    // Multi-line
                    var matchedMulti = delimiters.MultiLine
                        .Where(pair => i + pair.Item1.Length <= n && code.Substring(i, pair.Item1.Length) == pair.Item1)
                        .OrderByDescending(pair => pair.Item1.Length)
                        .FirstOrDefault();

                    if (matchedMulti != null)
                    {
                        string start = matchedMulti.Item1;
                        string end = matchedMulti.Item2;

                        result.Append(start);
                        i += start.Length;

                        if (removeComments)
                        {   
                            while (i < n && (i + end.Length > n || code.Substring(i, end.Length) != end))
                            {
                                i++;
                            }
                        }
                        else
                        {
                            string temp ="";
                            while (i < n && (i + end.Length > n || code.Substring(i, end.Length) != end))
                            {
                                temp+=code[i];
                                i++;
                            }
                            comments.Add(temp);
                        }
                        if (i + end.Length <= n)
                        {
                            result.Append(" ");
                            result.Append(end);
                            i += end.Length;
                        }

                        continue;
                    }

                result.Append(ch);
                i++;
            }


            return result.ToString();
        }

        public static List<string> TokenizeCode(string codeText)
        {
            codeText = codeText.Replace("\t", " ");
            var result = new List<string>();
            int i = 0;
            int n = codeText.Length;

            var current = new StringBuilder();
            string state = null; // "var", "num", "sym"

            var specialExceptions = new HashSet<char> { ',', ';', '.', '(', '{', '[', '"', '\'', ')', '}', ']' };

            while (i < n)
            {
                char ch = codeText[i];

                if (char.IsWhiteSpace(ch) || ch == '\n' || ch == '\r')
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                        state = null;
                    }
                    i++;
                    continue;
                }
                
                if ((ch == '\'' || ch =='`') && i > 0 && i < n - 1 && 
                    Utils.IsVariableChar(codeText[i - 1]) && Utils.IsVariableChar(codeText[i + 1]))
                {
                    current.Append(ch);
                    i++;
                    continue;
                }
                
                // if (")]}}".Contains(ch))
                // {
                //     i++;
                //     continue;
                // }
                //
                // if ("([{".Contains(ch))
                // {
                //     if (current.Length > 0)
                //     {
                //         result.Add(current.ToString());
                //         current.Clear();
                //         state = null;
                //     }
                //     result.Add(ch.ToString());
                //     i++;
                //     continue;
                // }
                if (specialExceptions.Contains(ch))
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                        state = null;
                    }
                    result.Add(ch.ToString());
                    i++;
                    continue;
                }

                if (Utils.IsVariableChar(ch))
                {
                    if (state == "var")
                    {
                        current.Append(ch);
                    }
                    else
                    {
                        if (current.Length > 0)
                            result.Add(current.ToString());
                        current.Clear();
                        current.Append(ch);
                        state = "var";
                    }
                    i++;
                    continue;
                }

                if (Utils.IsDigit(ch))
                {
                    if (state == "num")
                    {
                        current.Append(ch);
                    }
                    else if (state == "var")
                    {
                        current.Append(ch); // part of var like var1
                    }
                    else
                    {
                        if (current.Length > 0)
                            result.Add(current.ToString());
                        current.Clear();
                        current.Append(ch);
                        state = "num";
                    }
                    i++;
                    continue;
                }

                if (ch == '.' && state == "num" && i + 1 < n && Utils.IsDigit(codeText[i + 1]))
                {
                    current.Append('.');
                    i++;
                    continue;
                }

                if (!Utils.IsVariableChar(ch) && !Utils.IsDigit(ch))
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                        state = null;
                    }

                    current.Append(ch);
                    i++;
                    while (i < n)
                    {
                        char next = codeText[i];
                        if (char.IsWhiteSpace(next) || specialExceptions.Contains(next) || Utils.IsVariableChar(next) || Utils.IsDigit(next))
                            break;

                        current.Append(next);
                        i++;
                    }

                    result.Add(current.ToString());
                    current.Clear();
                    state = null;
                    continue;
                }

                if (state == "sym")
                {
                    current.Append(ch);
                }
                else
                {
                    if (current.Length > 0)
                        result.Add(current.ToString());
                    current.Clear();
                    current.Append(ch);
                    state = "sym";
                }
                i++;
            }

            if (current.Length > 0)
                result.Add(current.ToString());

            // Remove empty or whitespace-only tokens
            for (int j = result.Count - 1; j >= 0; j--)
            {
                result[j] = result[j].Replace(" ", "");
                if (string.IsNullOrWhiteSpace(result[j]))
                    result.RemoveAt(j);
            }

            return result;
        }
        
        private static string RemoveCommentsAndStrings(string code, CommentDelimiters delimiters, bool removeComments=true, bool removeStrings=true)
        {
            var result = new StringBuilder();
            int i = 0;
            int n = code.Length;
            char? inString = null;
            bool escape = false;

            Tuple<string, string> activeMultiLine = null;
            string activeSingleLine = null;
            
            while (i < n)
            {
                char ch = code[i];

                if (activeSingleLine == null && activeMultiLine == null)
                {
                    // Handle string literals
                    if (inString != null)
                    {
                        if (escape)
                        {
                            escape = false;
                            if (!removeStrings)
                            {
                                result.Append(ch);
                            }
                            i++; // Skip escaped character
                            continue;
                        }
                        if (ch == '\\')
                        {
                            escape = true;
                            i++;
                            continue;
                        }
                        if (ch == inString)
                        {
                            result.Append(ch);
                            inString = null; // Close string
                            i++;
                            continue;
                        }
                    
                        if (!removeStrings)
                        {
                            result.Append(ch);
                        }
                        i++;
                        continue;
                    }

                    // Start of a string
                    if (ch == '"' || ch == '\'')
                    {
                        inString = ch;
                        result.Append(ch); // Optionally keep the string start
                        i++;
                        continue;
                    }
                }

                if (inString == null)
                {
                    // Handle active single-line comment
                    if (activeSingleLine != null)
                    {
                        if (ch == '\n')
                        {
                            activeSingleLine = null;
                            result.Append(' ');
                        }
                        i++;
                        continue;
                    }

                    // Handle active multi-line comment
                    if (activeMultiLine != null)
                    {
                        var (start, end) = activeMultiLine;
                        if (i + end.Length <= n && code.Substring(i, end.Length) == end)
                        {
                            i += end.Length;
                            result.Append(end);
                            activeMultiLine = null;
                        }
                        else
                        {
                            i++;
                        }
                        continue;
                    }
                    
                    if (removeComments)
                    {
                        string matchedSingle = delimiters.SingleLine
                            .Where(sym => i + sym.Length <= n && code.Substring(i, sym.Length) == sym)
                            .OrderBy(sym => sym.Length)
                            .FirstOrDefault();

                        if (matchedSingle != null)
                        {
                            activeSingleLine = matchedSingle;
                            result.Append(activeSingleLine);
                            i += matchedSingle.Length;
                            continue;
                        }

                        // Check for new multi-line comment
                        Tuple<string, string> matchedMulti = delimiters.MultiLine
                            .Where(pair => i + pair.Item1.Length <= n && code.Substring(i, pair.Item1.Length) == pair.Item1)
                            .OrderBy(pair => pair.Item1.Length)
                            .FirstOrDefault();

                        if (matchedMulti != null)
                        {
                            activeMultiLine = matchedMulti;
                            result.Append(activeMultiLine.Item1);
                            i += matchedMulti.Item1.Length;
                            continue;
                        }
                    }
                }

                // Newlines -> space
                if (ch == '\r' || ch == '\n')
                {
                    result.Append(' ');
                    i++;
                    continue;
                }

                result.Append(ch);
                i++;
            }

            return result.ToString();
        }
    

        #endregion

        #region NaturalTextTokenize
        
        public static string[] TokenizeJapanese(string text)
        {
            var parameter = new MeCabParam
            {
                DicDir = Path.Combine(Path.GetTempPath(), "MeCab_dict"),
            };
            var tagger = MeCabTagger.Create(parameter);

            var parsed = tagger.Parse(text);

            return parsed.Split('\n').Select(x => x.Split('\t')[0]).ToArray();
        }
        public static string[] TokenizeChinese(string text)
        {
            var segmenter = new JiebaSegmenter();
            return segmenter.Cut(text, true).ToArray();
        }
        
        public static (int Hiragana, int Katakana, int Kanji) AnalyzeText(string input)
        {
            int hiragana = 0, katakana = 0, kanji = 0;
            foreach (char c in input)
            {
                if (c >= '\u3040' && c <= '\u309F') hiragana++;     
                else if ((c >= '\u30A0' && c <= '\u30FF') ||
                         (c >= '\u31F0' && c <= '\u31FF') ||
                         (c >= '\uFF65' && c <= '\uFF9F')) katakana++;
                else if ((c >= '\u4E00' && c <= '\u9FFF') ||
                         (c >= '\u3400' && c <= '\u4DBF')) kanji++;
            }
            return (hiragana, katakana, kanji);
        }
        
        public static string[] TrySplitWords(string inputText, HashSet<char> spacesList)
        {
            var analysis = AnalyzeText(inputText);

            if (analysis.Hiragana > 0 || analysis.Katakana > 0)
                return TokenizeJapanese(inputText);
            if (analysis.Kanji > 0)
                return TokenizeChinese(inputText);


            return inputText.Split(spacesList.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        }
        
        public static string[] TokenizeNatural(string inputText, HashSet<char> spacesList)
        {
            var text = _startSpaces.Replace(inputText, "");
            text = _endSpaces.Replace(text, "");
            var textContainSpaces = text.Any(x => spacesList.Except(_skipSpaces).Contains(x));

            var splitList = new List<char>(spacesList);
            splitList.Add('\n');
            if (textContainSpaces) {
                return text.Split(splitList.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            
            return TrySplitWords(text, spacesList);
        }
        
        public static string RemoveEndSigns(string word, HashSet<char> endSigns)
        {
            return new string(word.Where(x => !IsEndSign(x, endSigns) && (char.IsLetterOrDigit(x) || char.IsWhiteSpace(x))).ToArray());
        }
        public static bool IsEndSign(char ch, HashSet<char> endSigns) => endSigns.Contains(ch);

        public static bool NonRenderingCategories(char c) => nonRenderingCategories.Contains(char.GetUnicodeCategory(c));
        
        #endregion
    }
}

// = слова код1, коментар1, код2, коментар2
// != слова код1, код2, коментар1, коментар2
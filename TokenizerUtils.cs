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
    public enum CodeBlockType
    {
        CodeText,
        StringText,
        CommentText
    }
        
    public class CodeBlock
    {
        public CodeBlockType Type { get; } // "code", "string", "comment"
        public string Content { get; set; }

        public CodeBlock(CodeBlockType type, string content)
        {
            Type = type;
            Content = content;
        }

        public override string ToString()
        {
            return $"{Type}: {Content}";
        }
    }
    
    public class TokenizerUtils
    {
        public static readonly Regex SpacesInRow = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.Multiline);
        
        private static readonly char[] _skipSpaces = { '\u3000' };
        private static readonly HashSet<UnicodeCategory> nonRenderingCategories = new HashSet<UnicodeCategory>
        {
            UnicodeCategory.Control,
            UnicodeCategory.OtherNotAssigned,
            UnicodeCategory.Surrogate,
            UnicodeCategory.Format
        };
        
        #region GENERAL WORDS MANIPULATION

        public static string[] WordsIntersection(string[] words1, string[] words2)
        {
            HashSet<string> wordsSet = new HashSet<string>(words1);
            return words2.Where(word => wordsSet.Contains(word)).ToArray();
        }
        
        #endregion
        
        #region CodeTextTokenize
        
        public static List<CodeBlock> ParseCodeSegments(string code, CommentDelimiters delimiters)
        {
            var segments = new List<CodeBlock>();
            var current = new StringBuilder();
            CodeBlockType type = CodeBlockType.CodeText;

            int i = 0;
            int n = code.Length;
            bool inString = false;
            bool inSingleLineComment = false;
            bool inMultiLineComment = false;
            char stringChar = '\0';
            bool escape = false;
            string multiLineEnd = "";

            while (i < n)
            {
                char ch = code[i];

                if (inString)
                {
                    if (escape)
                    {
                        current.Append(ch);
                        escape = false;
                        i++;
                        continue;
                    }

                    if (ch == '\\')
                    {
                        current.Append(ch);
                        escape = true;
                        i++;
                        continue;
                    }

                    if (ch == stringChar)
                    {
                        segments.Add(new CodeBlock(CodeBlockType.StringText, current.ToString()));
                        current.Clear();
                        type = CodeBlockType.CodeText;
                        segments.Add(new CodeBlock(CodeBlockType.CodeText, ch.ToString()));
                        inString = false;
                    }
                    else
                    {
                        current.Append(ch);
                    }

                    i++;
                    continue;
                }

                if (inSingleLineComment)
                {
                    if (ch == '\n')
                    {
                        segments.Add(new CodeBlock(CodeBlockType.CommentText, current.ToString()));
                        current.Clear();
                        type = CodeBlockType.CodeText;
                        segments.Add(new CodeBlock(CodeBlockType.CodeText, ch.ToString())); // newline
                        inSingleLineComment = false;
                    }
                    else
                    {
                        current.Append(ch);
                    }
                    i++;
                    continue;
                }

                if (inMultiLineComment)
                {
                    if (i + multiLineEnd.Length <= n && code.Substring(i, multiLineEnd.Length) == multiLineEnd)
                    {
                        segments.Add(new CodeBlock(CodeBlockType.CommentText, current.ToString() ));
                        current.Clear();
                        type = CodeBlockType.CodeText;
                        segments.Add(new CodeBlock(CodeBlockType.CodeText, multiLineEnd ));
                        i += multiLineEnd.Length;
                        inMultiLineComment = false;
                        continue;
                    }
                    else
                    {
                        current.Append(ch);
                        i++;
                        continue;
                    }
                }

                // Detect string start
                if (ch == '"' || ch == '\'')
                {
                    if (current.Length > 0)
                    {
                        segments.Add(new CodeBlock(type, current.ToString() ));
                        current.Clear();
                    }
                    type = CodeBlockType.StringText;
                    stringChar = ch;
                    inString = true;
                    segments.Add(new CodeBlock(CodeBlockType.CodeText, ch.ToString() )); // opening quote
                    i++;
                    continue;
                }

                // Detect single-line comment
                var matchedSingle = delimiters.SingleLine
                    .FirstOrDefault(sym => i + sym.Length <= n && code.Substring(i, sym.Length) == sym);
                if (matchedSingle != null)
                {
                    if (current.Length > 0)
                    {
                        segments.Add(new CodeBlock(type, current.ToString() ));
                        current.Clear();
                    }
                    type = CodeBlockType.CommentText;
                    inSingleLineComment = true;
                    segments.Add(new CodeBlock(CodeBlockType.CodeText, matchedSingle )); // comment start
                    i += matchedSingle.Length;
                    continue;
                }


                // Detect multi-line comment
                var matchedMulti = delimiters.MultiLine
                    .FirstOrDefault(pair => i + pair.Item1.Length <= n && code.Substring(i, pair.Item1.Length) == pair.Item1);
                if (matchedMulti != null)
                {
                    if (current.Length > 0)
                    {
                        segments.Add(new CodeBlock(type, current.ToString() ));
                        current.Clear();
                    }
                    type = CodeBlockType.CommentText;
                    inMultiLineComment = true;
                    multiLineEnd = matchedMulti.Item2;
                    segments.Add(new CodeBlock(CodeBlockType.CodeText, matchedMulti.Item1 )); // comment start
                    i += matchedMulti.Item1.Length;
                    continue;
                }

                // Regular code
                current.Append(ch);
                i++;
            }

            if (current.Length > 0)
            {
                segments.Add(new CodeBlock(type, current.ToString() ));
            }

            return segments;
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
        
        public static List<CodeBlock> CleanBlocks(List<CodeBlock> list)
        {
            var cleaned = new List<CodeBlock>();

            foreach (var block in list)
            {
                // Skip empty or whitespace-only blocks
                if (string.IsNullOrWhiteSpace(block.Content))
                    continue;

                if (cleaned.Count > 0 && cleaned[cleaned.Count - 1].Type == block.Type)
                {
                    // Merge with previous block
                    cleaned[cleaned.Count - 1].Content += ' ' + block.Content;
                }
                else
                {
                    cleaned.Add(new CodeBlock(
                        block.Type,
                        block.Content
                    ));
                }
            }

            return cleaned;
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

            var spaceArray = spacesList.ToArray();
            return inputText.Split(spaceArray, StringSplitOptions.RemoveEmptyEntries);
        }
        
        public static string[] TokenizeNatural(string inputText, HashSet<char> spacesList)
        {
            if (string.IsNullOrWhiteSpace(inputText))
                return Array.Empty<string>();

            // Trim початку та кінця — швидше, ніж Regex
            string text = inputText.Trim();

            // Швидка перевірка на наявність роздільників
            bool hasSplitter = false;
            foreach (char ch in text)
            {
                if (spacesList.Contains(ch) && !_skipSpaces.Contains(ch))
                {
                    hasSplitter = true;
                    break;
                }
            }

            if (hasSplitter)
            {
                // Розбивка вручну — без Regex і зайвих алокацій
                List<string> tokens = new List<string>();
                int start = 0;
                for (int i = 0; i < text.Length; i++)
                {
                    if (spacesList.Contains(text[i]) || text[i] == '\n')
                    {
                        if (i > start)
                        {
                            string token = text.Substring(start, i - start);
                            if (!string.IsNullOrWhiteSpace(token))
                                tokens.Add(token);
                        }
                        start = i + 1;
                    }
                }

                if (start < text.Length)
                {
                    string token = text.Substring(start);
                    if (!string.IsNullOrWhiteSpace(token))
                        tokens.Add(token);
                }

                return tokens.ToArray();
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
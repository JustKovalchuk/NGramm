using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Globalization;
using System.Net;
using JiebaNet.Segmenter;
using NMeCab;
using System.Text;

namespace NGramm
{
    public class NgrammProcessor
    {
        private const int ProgressDivider = 1_000;
        bool ignore_spaces = false;
        bool ignore_nlines = false;
        bool ignore_ends = false;
        public static string signs = "\\|\"{}()[]=+_~!`@'#$-…%^&*:№:";
        public string ss = "\\|\"{}()[]=+_~`'@#$…%^&*№:";
        public static HashSet<char> endsigns = new HashSet<char>(".?!;。？！¿¡؟؛¿¡።༼⸮〽⋯…⸰;".ToCharArray());
        public static string endsignss = ",.?!;";
        public string rawTextorg = "";
        public Encoding fileEncoding;
        public int CountDesiredVariables = 0;
        public string unsignedTextorg = "";
        public string endsignedTextorg = "";
        public string raw = "";
        public static bool process_spaces = true;
        public static bool consequtive_spaces = true;
        public static bool show_NBS = true;
        public static bool ignore_case = true;
        public static bool ignore_punctuation = true;

        private static HashSet<char> spaces_list = new HashSet<char> { '\u0020', '\u00a0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200a', '\u202f', '\u205f', '\u3000', '\u200b' };
        private static char[] skip_spaces = new char[] { '\u3000' };
        private static string[] consequtive_spaces_pattern = spaces_list.Select(x => $"({x})+").ToArray();
        private readonly Regex spaces_pattern = new Regex($"({string.Join("|", consequtive_spaces_pattern)})", RegexOptions.Compiled); private static
            List<UnicodeCategory> nonRenderingCategories = new List<UnicodeCategory> {
            UnicodeCategory.Control,
            UnicodeCategory.OtherNotAssigned,
            UnicodeCategory.Surrogate,
            UnicodeCategory.Format
        };
        private static Regex startSpaces = new Regex(@"^\s+", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex endSpaces = new Regex(@"\s+$", RegexOptions.Compiled | RegexOptions.Multiline);
        private static Regex spacesInRow = new Regex(@"\s+", RegexOptions.Compiled | RegexOptions.Multiline);

        private ConcurrentBag<NGrammContainer> symbol_ngrams = new ConcurrentBag<NGrammContainer>();
        private ConcurrentBag<NGrammContainer> words_ngrams = new ConcurrentBag<NGrammContainer>();
        private ConcurrentBag<NGrammContainer> code_words_ngrams = new ConcurrentBag<NGrammContainer>();
        private ConcurrentBag<NGrammContainer> literal_ngrams = new ConcurrentBag<NGrammContainer>();

        private readonly string _filename;
        public readonly ProgressReporter progressReporter;

        public NgrammProcessor(string filename, ProgressReporter reporter)
        {

            _filename = filename;
            progressReporter = reporter;
            CountDesiredVariables = 0;
        }

        private static bool IsEndSign(char ch) => endsigns.Contains(ch);

        public async Task Preprocess(SimpleLogger _my_logger)
        {
            await Task.Run(() =>
            {
                Regex reg_exp = new Regex(@"(?<=(\w))--(?=(\w))");

                _my_logger.Print("Ініціалізація");
                progressReporter.StartNewOperation("Ініціалізація");
                progressReporter.MoveProgress(5);
                rawTextorg = reg_exp.Replace(File.ReadAllText(_filename), " ").Trim().Replace("\r", "");
                fileEncoding = Utils.GetEncoding(_filename);
                progressReporter.MoveProgress(5);
                var _raw = new StringBuilder();
                var _uns = new StringBuilder();
                var _ends = new StringBuilder();
                var progressMult = rawTextorg.Length / 90;
                var currentPr = 1;

                progressReporter.MoveProgress();
                for (int i = 0; i < rawTextorg.Length; i++)
                {
                    if (i >= progressMult * currentPr)
                    {
                        currentPr++;
                        progressReporter.MoveProgress();
                    }

                    char ch = rawTextorg[i];
/*                    if (!char.IsControl(ch))
                    {*/
                    bool added = false;
                    if (char.IsLetter(ch) || char.IsDigit(ch))
                    {
                        _raw.Append(ch);
                        _uns.Append(ch);
                        _ends.Append(ch);
                        ignore_spaces = ignore_ends = ignore_nlines = false;
                    }
                    else if (!ignore_spaces && (spaces_list.Contains(ch) || ch == '\t'))
                    {
                        _raw.Append(ch);
                        _uns.Append(ch);
                        _ends.Append(ch);
                        ignore_spaces = true;
                        ignore_ends = false;
                    }
                    else if ((ch == '\n') && !ignore_nlines)
                    {
                        _raw.Append(ch);
                        _uns.Append(ch);
                        _ends.Append(ch);
                        ignore_spaces = true;
                        ignore_nlines = true;
                        ignore_ends = false;
                    }
                    else if (ss.Contains(ch) || ch == '-')
                    {
                        _raw.Append(ch);
                    }
                    else if (!ignore_ends && IsEndSign(ch))
                    {
                        _raw.Append(ch);
                        _ends.Append(ch);
                    }
                    else
                    {
                        _raw.Append(ch);
                    }
                }
                //}

                rawTextorg = _raw.ToString(); // звичайний текст
                endsignedTextorg = _ends.ToString(); // з крапками
                unsignedTextorg = _uns.ToString();
                _raw.Clear();
                _ends.Clear();
                _uns.Clear();

                progressReporter.Finish();
            });
        }

        #region Literal ngramms

        public List<NGrammContainer> ProcessLiteralNGrammsInWindow(int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();

            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessLiteralNgrmmToContainer(unsignedTextorg.Substring(pos, windowSize), nn, true)).ToList(), n);
                res.Add(cts);
                pos += windowStep;
            }
            return res;
        }

        public Task ProcessLiteralNGramms(int n) =>
            Task.Run(() =>
            {
                progressReporter.StartNewOperation($"Обчислення буквенних н-грамм від 1 до {n}");

                var text = RemoveConsequtiveSpaces(unsignedTextorg);
                CountDesiredVariables = text.Length;
                var progressMult = text.Length * n / 95;
                ClearAllNGrammContainers();

                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessLiteralNgrmmToContainer(text, nn, false, progressMult);
                    literal_ngrams.Add(ct);
                });

                literal_ngrams = new ConcurrentBag<NGrammContainer>(literal_ngrams.OrderByDescending(w => w.n));
                progressReporter.Finish();
            });

        private NGrammContainer ProcessLiteralNgrmmToContainer(string text, int n, bool countDigits, int progressMul = 0)
        {
            var container = new NGrammContainer(n);
            bool breaked;
            char ch;
            int ct = 1;

            text = spacesInRow.Replace(text, " ");

            for (int i = 0; i < text.Length; i++)
            {
                if (progressMul != 0 && i > progressMul * ct)
                {
                    progressReporter.MoveProgress();
                    ct++;
                }

                if (i + n - 1 < text.Length)
                {
                    breaked = false;
                    string ngram = string.Empty;
                    for (int k = 0; k < n; k++)
                    {
                        ch = text[i + k];

                        var isSpace = spaces_pattern.Match(ch.ToString()).Success;
                        var notPrintableSymbol = NonRenderingCategories(ch);

                        if (char.IsControl(ch))
                        {
                            breaked = true;
                            break;
                        }

                        if (process_spaces)
                        {
                            if (!IsEndSign(ch) && notPrintableSymbol)
                            {
                                if (isSpace)
                                    ngram += ' ';
                                else 
                                    ngram += ch;
                            }
                            else
                            {
                                breaked = true;
                                break;
                            }

                        }
                        else
                        {
                            if (!IsEndSign(ch) && !spaces_list.Contains(ch) && !notPrintableSymbol)
                            {
                                ngram += ch;
                            }
                            else
                            {
                                breaked = true;
                                break;
                            }
                        }
                    }

                    if (!breaked)
                    {
                        container.Add(ignore_case ? ngram.ToLower() : ngram);
                    }
                }
            }

            container.Process();
            return container;
        }

        public int GetLiteralCount(bool countSpaces)
        {
            var text = spacesInRow.Replace(unsignedTextorg, " ");

            return text.Count(c => char.IsLetter(c) || (countSpaces && spaces_list.Contains(c)));
        }

        #endregion

        #region Symbol ngramms

        public List<NGrammContainer> ProcessSymbolNGrammsInWindow(int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();

            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessSymbolNgrmmToContainer(rawTextorg.Substring(pos, windowSize), nn)).ToList(), n);
                res.Add(cts);
                pos += windowStep;
            }
            return res;
        }

        public Task ProcessSymbolNGramms(int n) =>
            Task.Run(() =>
            {
                progressReporter.StartNewOperation($"Обчислення символьних н-грамм від 1 до {n}");
                var text = rawTextorg;

                if (!consequtive_spaces)
                {
                    text = RemoveConsequtiveSpaces(text);

                    if (!show_NBS)
                    {
                        text = spacesInRow.Replace(text, " ");
                    }
                }


                CountDesiredVariables = text.Length;
                ClearAllNGrammContainers();
                int progressMul = text.Length / 95;
                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessSymbolNgrmmToContainer(text, nn, progressMul);
                    symbol_ngrams.Add(ct);
                });

                symbol_ngrams = new ConcurrentBag<NGrammContainer>(symbol_ngrams.OrderByDescending(w => w.n));
                progressReporter.Finish();
            });

        private NGrammContainer ProcessSymbolNgrmmToContainer(string text, int n, int reportMul = 0)
        {
            var container = new NGrammContainer(n);
            bool breaked;
            char ch;
            int ct = 1;

            for (int i = 0; i < text.Length; i++)
            {
                if (reportMul != 0 && i> reportMul * ct)
                {
                    progressReporter.MoveProgress();
                    ct++;
                }

                if (i + n - 1 < text.Length)
                {
                    breaked = false;
                    StringBuilder ngramBuilder = new StringBuilder();
                    for (int k = 0; k < n; k++)
                    {
                        ch = text[i + k];
                        var isSpace = spaces_pattern.Match(ch.ToString()).Success;
                        var notPrintableSymbol = NonRenderingCategories(ch);

                        if (process_spaces && !notPrintableSymbol)
                        {
                            if (!show_NBS && isSpace)
                            {
                                ngramBuilder.Append(' ');
                            } else
                                ngramBuilder.Append(ch);
                        }
                        else
                        {
                            if (show_NBS && (isSpace || notPrintableSymbol))
                            {
                                ngramBuilder.Append(ch);
                            } else if(!isSpace && !notPrintableSymbol)
                            {
                                ngramBuilder.Append(ch);
                            }
                            else
                            {
                                breaked = true;
                                break;
                            }
                        }
                    }
                    if (!breaked)
                    {
                        var ngram = ngramBuilder.ToString();
                        ngramBuilder.Clear();
                        container.Add(ignore_case ? ngram.ToLower() : ngram);
                    }
                }
            }

            container.Process();
            return container;
        }

        public int GetSymbolsCount(bool countSpaces) {
            var text = rawTextorg;

            if (!consequtive_spaces)
            {
                text = RemoveConsequtiveSpaces(text);

                if (!show_NBS)
                {
                    text = spacesInRow.Replace(text, " ");
                }
            }

            return countSpaces ? text.Length : text.Count(c => !spaces_list.Contains(c));
        }

        #endregion

        #region Word ngramms

        public List<NGrammContainer> ProcessWordNGrammsInWindow(string[] words ,int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();
            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var wrds = words.Skip(pos).Take(windowSize).ToArray();
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessWordNgrmmToContainer(wrds, nn, false, ignore_case)).ToList(), n);
                res.Add(cts);
                
                pos += windowStep;
            }
            return res;
        }

        public Task ProcessWordNGramms(int n) 
        {
            return Task.Run(() =>
            {
                progressReporter.StartNewOperation($"Обчислення словесних н-грамм від 1 до {n}");
                progressReporter.MoveProgress();

                var words = Words(ignore_punctuation ? unsignedTextorg : endsignedTextorg);
                CountDesiredVariables = words.Length;
                ClearAllNGrammContainers();
                int progressMult = words.Length / 95;

                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessWordNgrmmToContainer(words, nn, ignore_punctuation, ignore_case, progressMult);
                    words_ngrams.Add(ct);
                });

                words_ngrams = new ConcurrentBag<NGrammContainer>(words_ngrams.OrderByDescending(w => w.n));
                progressReporter.Finish();
            });
        }
        
        public int GetWordsCount() => Words(ignore_punctuation ? unsignedTextorg : endsignedTextorg).Length;
        public int GetCodeWordsCount() => TokenizeCode(File.ReadAllText(_filename, fileEncoding)).ToArray().Length;

        private NGrammContainer ProcessWordNgrmmToContainer(string[] words, int n, bool skipss, bool ignoreCase, int progressMul = 0, bool isCode = false)
        {
            var container = new NGrammContainer(n);
            bool breaked;
            int ct = 1;

            for (int i = 0; i < words.Length; i++)
            {
                if (progressMul != 0 && i> progressMul * ct)
                {
                    progressReporter.MoveProgress();
                    ct++;
                }

                if (i + n - 1 < words.Length)
                {
                    breaked = false;
                    StringBuilder ngramBuilder = new StringBuilder();
                    for (int k = 0; k < n; k++)
                    {
                        var word = words[i + k];

                        if (string.IsNullOrWhiteSpace(word) || word.All(x => NonRenderingCategories(x)))
                        {
                            breaked = true;
                            break;
                        }

                        if (skipss)
                        {
                            if (word.Length > 1 && IsEndSign(word[word.Length - 1]))
                                word = word.Remove(word.Length - 1);

                            if (ngramBuilder.Length == 0)
                                ngramBuilder.Append(word);
                            else
                                ngramBuilder.Append($" {word}");
                        }
                        else
                        {
                            if (ngramBuilder.Length == 0)
                            {
                                ngramBuilder.Append(word);
                            }
                            else 
                            if (!IsEndSign(ngramBuilder[ngramBuilder.Length - 1])) ngramBuilder.Append($" {word}");
                            else
                            {
                                breaked = true;
                                break;
                            }
                        }
                    }
                    if (!breaked)
                    {
                        
                        var ngram = ngramBuilder.ToString();
                        if (!isCode)
                        {
                            ngram = RemoveEndSigns(ngram);
                        }
                        ngramBuilder.Clear();
                        container.Add(ignoreCase ? ngram.ToLower() : ngram);
                    }
                }
            }

            container.Process();
            return container;
        }

        #endregion

        #region CodeWords ngrams

        public List<NGrammContainer> ProcessCodeWordNGrammsInWindow(string[] words ,int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();
            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var wrds = words.Skip(pos).Take(windowSize).ToArray();
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessWordNgrmmToContainer(wrds, nn, false, false,  0, true)).ToList(), n);
                res.Add(cts);
                
                pos += windowStep;
            }
            return res;
        }
        
        public Task ProcessCodeWordNGramms(int n, SimpleLogger myLogger) 
        {
            return Task.Run(() =>
            {
                progressReporter.StartNewOperation($"Обчислення словесних н-грамм в коді від 1 до {n}");
                progressReporter.MoveProgress();

                string codeSample = File.ReadAllText(_filename, fileEncoding);
                var tokens = TokenizeCode(codeSample);
                var words = tokens.ToArray();
                CountDesiredVariables = words.Length;
                ClearAllNGrammContainers();
                int progressMult = words.Length / 95;

                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessWordNgrmmToContainer(words, nn, false, false,  progressMult, true);
                    code_words_ngrams.Add(ct);
                });

                code_words_ngrams = new ConcurrentBag<NGrammContainer>(code_words_ngrams.OrderByDescending(w => w.n));
                progressReporter.Finish();
            });
        }

        #endregion

        #region CodeWordsPreproces
        
        public static string RemoveCommentsAndStrings(string code)
        {
            var result = new StringBuilder();
            int i = 0;
            int n = code.Length;
            char? inString = null;
            bool escape = false;

            Console.WriteLine("modified by LiberMaeotis creators (GDG 2025)");
            while (i < n)
            {
                char ch = code[i];

                // If inside a string literal
                if (inString != null)
                {
                    if (escape)
                    {
                        escape = false;
                        result.Append(""); // remove escaped char
                    }
                    else if (ch == '\\')
                    {
                        escape = true;
                        result.Append(""); // remove backslash
                    }
                    else if (ch == inString)
                    {
                        result.Append(inString); // close quote
                        inString = null;
                    }
                    else
                    {
                        result.Append(""); // remove inside string
                    }
                    i++;
                    continue;
                }

                // Start of a string
                if (ch == '"' || ch == '\'')
                {
                    inString = ch;
                    result.Append(ch);
                    i++;
                    continue;
                }

                // Start of // comment
                if (ch == '/' && i + 1 < n && code[i + 1] == '/')
                {
                    while (i < n && code[i] != '\n')
                        i++;
                    result.Append(' ');
                    continue;
                }

                // Start of /* */ comment
                if (ch == '/' && i + 1 < n && code[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < n && !(code[i] == '*' && code[i + 1] == '/'))
                        i++;
                    i += 2; // skip '*/'
                    result.Append(' ');
                    continue;
                }

                // Start of # comment
                if (ch == '#')
                {
                    while (i < n && code[i] != '\n')
                        i++;
                    result.Append(' ');
                    continue;
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
        
        public static List<string> AddClosingBracket(List<string> code)
        {
            for (int i = 0; i < code.Count; i++)
            {
                switch (code[i])
                {
                    case "{":
                        code[i] = code[i] + "}";
                        break;
                    case "[":
                        code[i] = code[i] + "]";
                        break;
                    case "(":
                        code[i] = code[i] + ")";
                        break;
                }
            }
            return code;
        }
        
        public static List<string> TokenizeCode(string code)
        {
            code = code.Replace("\t", " ");
            code = RemoveCommentsAndStrings(code); // reuse earlier C# version
            var result = new List<string>();
            int i = 0;
            int n = code.Length;

            var current = new StringBuilder();
            string state = null; // "var", "num", "sym"

            var specialExceptions = new HashSet<char> { ',', ';', '.', '(', '{', '[', '"', '\'', ')', '}', ']' };

            while (i < n)
            {
                char ch = code[i];

                if (char.IsWhiteSpace(ch) || ch == '\n' || ch == '\r')
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current = new StringBuilder();
                        state = null;
                    }
                    i++;
                    continue;
                }

                if (")]}}".Contains(ch))
                {
                    i++;
                    continue;
                }

                if ("([{".Contains(ch))
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

                if (ch == '.' && state == "num" && i + 1 < n && Utils.IsDigit(code[i + 1]))
                {
                    current.Append('.');
                    i++;
                    continue;
                }

                if (!specialExceptions.Contains(ch) && !Utils.IsVariableChar(ch) && !Utils.IsDigit(ch))
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
                        char next = code[i];
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

                if (ch == '"')
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }

                    current.Append(ch);
                    i++;
                    while (i < n && code[i] != '"')
                    {
                        current.Append(code[i]);
                        i++;
                    }
                    current.Append('"');
                    result.Add(current.ToString());
                    current.Clear();
                    i++;
                    continue;
                }

                if (ch == '\'')
                {
                    if (current.Length > 0)
                    {
                        result.Add(current.ToString());
                        current.Clear();
                    }

                    current.Append(ch);
                    i++;
                    while (i < n && code[i] != '\'')
                    {
                        current.Append(code[i]);
                        i++;
                    }
                    current.Append('\'');
                    result.Add(current.ToString());
                    current.Clear();
                    i++;
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

            return AddClosingBracket(result);
        }
        
        #endregion
        
        private string RemoveEndSigns(string word)
        {
            return new string(word.Where(x => !IsEndSign(x) && (char.IsLetterOrDigit(x) || char.IsWhiteSpace(x))).ToArray());
        }

        public IReadOnlyCollection<NGrammContainer> GetLiteralNgrams() => literal_ngrams;

        public IReadOnlyCollection<NGrammContainer> GetSymbolNgrams() => symbol_ngrams;

        public IReadOnlyCollection<NGrammContainer> GetWordsNgrams() => words_ngrams;
        
        public IReadOnlyCollection<NGrammContainer> GetCodeWordsNgrams() => code_words_ngrams;

        public string[] CodeWords()
        {
            string codeSample = File.ReadAllText(_filename, fileEncoding);
            var tokens = TokenizeCode(codeSample);
            var words = tokens.ToArray();
            return words;
        }

        public static string[] Words(string inputText)
        {
            var text = startSpaces.Replace(inputText, "");
            text = endSpaces.Replace(text, "");
            var textContainSpaces = text.Any(x => spaces_list.Except(skip_spaces).Contains(x));

            var splitList = new List<char>(spaces_list);
            splitList.Add('\n');
            if (textContainSpaces) {
                return text.Split(splitList.ToArray(), StringSplitOptions.RemoveEmptyEntries);
            }
            
            return TrySplitWords(text);
        }

        public static string[] TrySplitWords(string inputText)
        {
            var analysis = AnalyzeText(inputText);

            if (analysis.Hiragana > 0 || analysis.Katakana > 0)
                return TokenizeJapanese(inputText);
            else if (analysis.Kanji > 0)
                return TokenizeChinese(inputText);


            return inputText.Split(spaces_list.ToArray(), StringSplitOptions.RemoveEmptyEntries);
        }

        private static(int Hiragana, int Katakana, int Kanji) AnalyzeText(string input)
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


        private static string[] TokenizeJapanese(string text)
        {
            var parameter = new MeCabParam
            {
                DicDir = Path.Combine(Path.GetTempPath(), "MeCab_dict"),
            };
            var tagger = MeCabTagger.Create(parameter);

            var parsed = tagger.Parse(text);

            return parsed.Split('\n').Select(x => x.Split('\t')[0]).ToArray();
        }
        private static string[] TokenizeChinese(string text)
        {
            var segmenter = new JiebaSegmenter();
            return segmenter.Cut(text, true).ToArray();
        }


        private void ClearAllNGrammContainers()
        {
            symbol_ngrams = new ConcurrentBag<NGrammContainer>();
            literal_ngrams = new ConcurrentBag<NGrammContainer>();
            words_ngrams = new ConcurrentBag<NGrammContainer>();
            code_words_ngrams = new ConcurrentBag<NGrammContainer>();
        }

        private string RemoveConsequtiveSpaces(string input)
        {
            var result = input;
            foreach (string item in consequtive_spaces_pattern) {
                var regex = new Regex(item, RegexOptions.Multiline);

                result = regex.Replace(result, match =>
                {
                    return match.Value[0].ToString();
                });

            }

            return result;
        }
        private bool NonRenderingCategories(char c)
        {
            return nonRenderingCategories.Contains(char.GetUnicodeCategory(c));
        }
    }
}

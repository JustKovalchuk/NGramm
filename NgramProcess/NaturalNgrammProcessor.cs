using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;

namespace NGramm
{
    public class NaturalNgrammProcessor : BasicNgrammProcessor
    {
        private string ss = "\\|\"{}()[]=+_~`'@#$…%^&*№:";
        private HashSet<char> endsigns = new HashSet<char>(".?!;。？！¿¡؟؛¿¡።༼⸮〽⋯…⸰;".ToCharArray());
        public override HashSet<char> Endsigns { get => endsigns; set => endsigns = value; } 
        
        public string _rawTextorg = "";
        public string _unsignedTextorg = "";
        public string _endsignedTextorg = "";
        
        public string raw = "";
        public static bool consequtive_spaces = true;
        public static bool show_NBS = true;

        private static HashSet<char> spaces_list = new HashSet<char> { '\u0020', '\u00a0', '\u1680', '\u2000', '\u2001', '\u2002', '\u2003', '\u2004', '\u2005', '\u2006', '\u2007', '\u2008', '\u2009', '\u200a', '\u202f', '\u205f', '\u3000', '\u200b' };

        private static string[] consequtive_spaces_pattern = spaces_list.Select(x => $"({x})+").ToArray();
        private readonly Regex spaces_pattern = new Regex($"({string.Join("|", consequtive_spaces_pattern)})", RegexOptions.Compiled);
        
        public NaturalNgrammProcessor(string filename, ProgressReporter reporter, string textToProcess): base(filename, reporter, textToProcess) { }
        
        public override async Task PreprocessAsync()
        {
            await Task.Run(() =>
            {
                bool ignoreSpaces = false;
                bool ignoreNlines = false;
                bool ignoreEnds = false;
                
                Regex regExp = new Regex(@"(?<=(\w))--(?=(\w))");

                MyProgressReporter?.StartNewOperation("Ініціалізація");
                MyProgressReporter?.MoveProgress(5);
                
                _rawTextorg = regExp.Replace(readTextToProcess, " ").Trim().Replace("\r", "");
                
                MyProgressReporter?.MoveProgress(5);
                
                var _raw = new StringBuilder();
                var _uns = new StringBuilder();
                var _ends = new StringBuilder();
                var progressMult = _rawTextorg.Length / 90;
                var currentPr = 1;

                MyProgressReporter?.MoveProgress();
                for (int i = 0; i < _rawTextorg.Length; i++)
                {
                    if (i >= progressMult * currentPr)
                    {
                        currentPr++;
                        MyProgressReporter?.MoveProgress();
                    }

                    char ch = _rawTextorg[i];
                    // if (!char.IsControl(ch))
                    // {
                    if (char.IsLetter(ch) || char.IsDigit(ch))
                    {
                        _raw.Append(ch);
                        _uns.Append(ch);
                        _ends.Append(ch);
                        ignoreSpaces = ignoreEnds = ignoreNlines = false;
                    }
                    else if (!ignoreSpaces && (spaces_list.Contains(ch) || ch == '\t'))
                    {
                        _raw.Append(ch);
                        _uns.Append(ch);
                        _ends.Append(ch);
                        ignoreSpaces = true;
                        ignoreEnds = false;
                    }
                    else if ((ch == '\n') && !ignoreNlines)
                    {
                        _raw.Append(ch);
                        _uns.Append(ch);
                        _ends.Append(ch);
                        ignoreSpaces = true;
                        ignoreNlines = true;
                        ignoreEnds = false;
                    }
                    else if (ss.Contains(ch) || ch == '-')
                    {
                        _raw.Append(ch);
                    }
                    else if (!ignoreEnds && TokenizerUtils.IsEndSign(ch, Endsigns))
                    {
                        _raw.Append(ch);
                        _ends.Append(ch);
                    }
                    else
                    {
                        _raw.Append(ch);
                    }
                    //}
                }

                _rawTextorg = _raw.ToString(); // звичайний текст
                _endsignedTextorg = _ends.ToString(); // з крапками
                _unsignedTextorg = _uns.ToString(); // без розділових
                _raw.Clear();
                _ends.Clear();
                _uns.Clear();

                MyProgressReporter?.Finish();
            });
        }

        #region Literal ngramms

        public override List<NGrammContainer> ProcessLiteralNGrammsInWindow(int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();

            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessLiteralNgrmmToContainer(_unsignedTextorg.Substring(pos, windowSize), nn, true)).ToList(), n);
                res.Add(cts);
                pos += windowStep;
            }
            return res;
        }

        public override Task ProcessLiteralNGramms(int n) =>
            Task.Run(() =>
            {
                MyProgressReporter?.StartNewOperation($"Обчислення буквенних н-грамм від 1 до {n}");

                var text = RemoveConsequtiveSpaces(_unsignedTextorg);
                CountDesiredVariables = text.Length;
                var progressMult = text.Length * n / 95;
                ClearAllNGrammContainers();

                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessLiteralNgrmmToContainer(text, nn, false, progressMult);
                    literal_ngrams.Add(ct);
                });

                literal_ngrams = new ConcurrentBag<NGrammContainer>(literal_ngrams.OrderByDescending(w => w.n));
                MyProgressReporter?.Finish();
            });

        private NGrammContainer ProcessLiteralNgrmmToContainer(string text, int n, bool countDigits, int progressMul = 0)
        {
            var container = new NGrammContainer(n);
            bool breaked;
            char ch;
            int ct = 1;

            text = TokenizerUtils.SpacesInRow.Replace(text, " ");

            for (int i = 0; i < text.Length; i++)
            {
                if (progressMul != 0 && i > progressMul * ct)
                {
                    MyProgressReporter?.MoveProgress();
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
                        var notPrintableSymbol = TokenizerUtils.NonRenderingCategories(ch);

                        if (char.IsControl(ch))
                        {
                            breaked = true;
                            break;
                        }

                        if (process_spaces)
                        {
                            if (!TokenizerUtils.IsEndSign(ch, Endsigns) && notPrintableSymbol)
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
                            if (!TokenizerUtils.IsEndSign(ch, Endsigns) && !spaces_list.Contains(ch) && !notPrintableSymbol)
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
                        container.Add(ignoreCase ? ngram.ToLower() : ngram);
                    }
                }
            }

            container.Process();
            return container;
        }

        public override int GetLiteralCount(bool countSpaces)
        {
            var text = TokenizerUtils.SpacesInRow.Replace(_unsignedTextorg, " ");

            return text.Count(c => char.IsLetter(c) || (countSpaces && spaces_list.Contains(c)));
        }

        #endregion

        #region Symbol ngramms

        public override List<NGrammContainer> ProcessSymbolNGrammsInWindow(int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();

            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessSymbolNgrmmToContainer(_rawTextorg.Substring(pos, windowSize), nn)).ToList(), n);
                res.Add(cts);
                pos += windowStep;
            }
            return res;
        }

        public override Task ProcessSymbolNGramms(int n) =>
            Task.Run(() =>
            {
                MyProgressReporter?.StartNewOperation($"Обчислення символьних н-грамм від 1 до {n}");
                var text = _rawTextorg;

                if (!consequtive_spaces)
                {
                    text = RemoveConsequtiveSpaces(text);

                    if (!show_NBS)
                    {
                        text = TokenizerUtils.SpacesInRow.Replace(text, " ");
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
                MyProgressReporter?.Finish();
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
                    MyProgressReporter?.MoveProgress();
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
                        var notPrintableSymbol = TokenizerUtils.NonRenderingCategories(ch);

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
                        container.Add(ignoreCase ? ngram.ToLower() : ngram);
                    }
                }
            }

            container.Process();
            return container;
        }

        public override int GetSymbolsCount(bool countSpaces) {
            var text = _rawTextorg;

            if (!consequtive_spaces)
            {
                text = RemoveConsequtiveSpaces(text);

                if (!show_NBS)
                {
                    text = TokenizerUtils.SpacesInRow.Replace(text, " ");
                }
            }

            return countSpaces ? text.Length : text.Count(c => !spaces_list.Contains(c));
        }

        #endregion

        #region Word ngramms

        public override List<NGrammContainer> ProcessWordNGrammsInWindow(string[] words ,int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();
            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var wrds = words.Skip(pos).Take(windowSize).ToArray();
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessWordNgrmmToContainer(wrds, nn, false, ignoreCase)).ToList(), n);
                res.Add(cts);
                
                pos += windowStep;
            }
            return res;
        }

        public override Task ProcessWordNGramms(int n) 
        {
            return Task.Run(() =>
            {
                MyProgressReporter?.StartNewOperation($"Обчислення лексичних n-грам від 1 до {n}");
                MyProgressReporter?.MoveProgress();

                var words = Words();
                CountDesiredVariables = words.Length;
                ClearAllNGrammContainers();
                int progressMult = words.Length / 95;

                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessWordNgrmmToContainer(words, nn, ignorePunctuation, ignoreCase, progressMult);
                    words_ngrams.Add(ct);
                });

                words_ngrams = new ConcurrentBag<NGrammContainer>(words_ngrams.OrderByDescending(w => w.n));
                MyProgressReporter?.Finish();
            });
        }

        public override NGrammContainer IntermediateProcessWordNgrmmToContainer(
            NGrammContainer container, string[] words, int n, bool skipss, bool ignoreCase, int progressMul)
        {
            string[] currentProcessorWords = Words();
            words = TokenizerUtils.WordsIntersection(words, currentProcessorWords);
            
            bool breaked;
            int ct = 1;

            for (int i = 0; i < words.Length; i++)
            {
                if (progressMul != 0 && i > progressMul * ct)
                {
                    MyProgressReporter?.MoveProgress();;
                    ct++;
                }

                if (i + n - 1 < words.Length)
                {
                    breaked = false;
                    StringBuilder ngramBuilder = new StringBuilder();
                    for (int k = 0; k < n; k++)
                    {
                        var word = words[i + k];

                        if (string.IsNullOrWhiteSpace(word) || word.All(x => TokenizerUtils.NonRenderingCategories(x)))
                        {
                            breaked = true;
                            break;
                        }

                        if (skipss)
                        {
                            if (word.Length > 1 && TokenizerUtils.IsEndSign(word[word.Length - 1], Endsigns))
                                word = word.Remove(word.Length - 1);

                            if (ngramBuilder.Length > 0)
                                ngramBuilder.Append(' ');
                            ngramBuilder.Append(word);
                        }
                        else
                        {
                            if (ngramBuilder.Length == 0)
                            {
                                ngramBuilder.Append(word);
                            }
                            else if (!TokenizerUtils.IsEndSign(ngramBuilder[ngramBuilder.Length - 1], Endsigns)) 
                                ngramBuilder.Append($" {word}");
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
                        ngram = TokenizerUtils.RemoveEndSigns(ngram, Endsigns);
                        ngramBuilder.Clear();
                        container.Add(ignoreCase ? ngram.ToLower() : ngram);
                    }
                }
            }

            return container;
        }

        protected override NGrammContainer ProcessWordNgrmmToContainer(string[] words, int n, bool skipss, bool ignoreCase, int progressMul = 0)
        {
            var container = IntermediateProcessWordNgrmmToContainer(
                new NGrammContainer(n), words, n, skipss, ignoreCase, progressMul);
            container.Process();
            return container;
        }

        #endregion
        
        public override string GetLiteralSubstring(int startPos, int length) =>
            _unsignedTextorg.Substring(startPos, length);
        public override string GetSymbolsSubstring(int startPos, int length) =>
            _rawTextorg.Substring(startPos, length);
        
        public override string[] Words() => TokenizerUtils.TokenizeNatural(
            ignorePunctuation ? _unsignedTextorg : _endsignedTextorg, spaces_list);
        
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
    }
}

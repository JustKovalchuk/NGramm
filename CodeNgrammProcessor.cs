using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;

namespace NGramm
{
    public class CodeNgrammProcessor: NgrammProcessor
    {
        public static bool removeCodeComments;
        public static bool removeCodeStrings;
        private bool canRemoveComments = true;
        
        public bool CanRemoveComments => canRemoveComments;

        private string codeSample = "";
        private CommentDelimiters _delimiter;
        
        public CodeNgrammProcessor(string filename, ProgressReporter reporter, bool removeComments, bool removeStrings) 
            : base(filename, reporter)
        {
            Console.WriteLine("modified by LiberMaeotis creators (GDG 2025)");
            removeCodeComments = removeComments;
            removeCodeStrings = removeStrings;
        }

        public override async Task Preprocess()
        {
            await base.Preprocess();
            
            codeSample = File.ReadAllText(_filename, fileEncoding);
                
            Utils.GetCommentsByExtension(_filename, out CommentDelimiters delimiter, out bool canRecognizeComments);
            canRemoveComments = canRecognizeComments;
            _delimiter = delimiter;
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
        
        // public static List<string> AddClosingBracket(List<string> code)
        // {
        //     for (int i = 0; i < code.Count; i++)
        //     {
        //         switch (code[i])
        //         {
        //             case "{":
        //                 code[i] = code[i] + "}";
        //                 break;
        //             case "[":
        //                 code[i] = code[i] + "]";
        //                 break;
        //             case "(":
        //                 code[i] = code[i] + ")";
        //                 break;
        //         }
        //     }
        //     return code;
        // }
        
        private static List<string> TokenizeCode(string code, CommentDelimiters delimiter, bool removeComments=true, bool removeStrings=true)
        {
            code = code.Replace("\t", " ");
            code = RemoveCommentsAndStrings(code, delimiter, removeComments, removeStrings); // reuse earlier C# version
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
                        current.Clear();
                        state = null;
                    }
                    i++;
                    continue;
                }
                
                if ((ch == '\'' || ch =='`') && i > 0 && i < n - 1 && 
                    Utils.IsVariableChar(code[i - 1]) && Utils.IsVariableChar(code[i + 1]))
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

                if (ch == '.' && state == "num" && i + 1 < n && Utils.IsDigit(code[i + 1]))
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

            return result;
        }
        
        public override List<NGrammContainer> ProcessWordNGrammsInWindow(string[] words ,int n, int windowSize, int windowStep, int startPos, int endPos)
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
        
        public override Task ProcessWordNGramms(int n) 
        {
            return Task.Run(() =>
            {
                progressReporter.StartNewOperation($"Обчислення словесних н-грамм (для програмного коду) від 1 до {n}");
                progressReporter.MoveProgress();
                
                var tokens = TokenizeCode(codeSample, _delimiter, removeCodeComments, removeCodeStrings);
                var words = tokens.ToArray();
                CountDesiredVariables = words.Length;
                ClearAllNGrammContainers();
                int progressMult = words.Length / 95;

                Parallel.For(1, n + 1, PerformanceSettings.ParallelOpt, nn =>
                {
                    var ct = ProcessWordNgrmmToContainer(words, nn, false, false,  progressMult, true);
                    words_ngrams.Add(ct);
                });

                words_ngrams = new ConcurrentBag<NGrammContainer>(words_ngrams.OrderByDescending(w => w.n));
                progressReporter.Finish();
            });
        }

        public override string[] WordsNotStatic()
        {
            var tokens = TokenizeCode(codeSample, _delimiter, removeCodeComments, removeCodeStrings);
            var words = tokens.ToArray();
            return words;
        }
        
        public override int GetWordsCount() => TokenizeCode(codeSample, _delimiter, removeCodeComments, removeCodeStrings).ToArray().Length;
    }
}
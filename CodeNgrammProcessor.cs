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
        
        public CodeNgrammProcessor(string filename, ProgressReporter reporter, bool removeComments, bool removeStrings) 
            : base(filename, reporter)
        {
            Console.WriteLine("modified by LiberMaeotis creators (GDG 2025)");
            removeCodeComments = removeComments;
            removeCodeStrings = removeStrings;
        }
        
        public static string RemoveCommentsAndStrings(string code, bool removeComments=true, bool removeStrings=true)
        {
            var result = new StringBuilder();
            int i = 0;
            int n = code.Length;
            char? inString = null;
            bool escape = false;

            while (i < n)
            {
                char ch = code[i];

                if (removeStrings)
                {
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
                }

                if (removeComments)
                {
                    var isSlashComment = ch == '/' && i + 1 < n && code[i + 1] == '/';
                    var isHashtagComment = ch == '#';
                    // Start of // and # comment
                    if (isSlashComment || isHashtagComment)
                    {
                        while (i < n && code[i] != '\n')
                            i++;
                        if (isSlashComment)
                            result.Append(" //");
                        else if (isHashtagComment)
                            result.Append(" #");
                        else
                            result.Append(" ");
                        continue;
                    }

                    // Start of /* */ comment
                    if (ch == '/' && i + 1 < n && code[i + 1] == '*')
                    {
                        i += 2;
                        while (i + 1 < n && !(code[i] == '*' && code[i + 1] == '/'))
                            i++;
                        i += 2; // skip '*/'
                        result.Append(" /* */");
                        continue;
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
        
        public static List<string> TokenizeCode(string code, bool removeComments=true, bool removeStrings=true)
        {
            code = code.Replace("\t", " ");
            code = RemoveCommentsAndStrings(code, removeComments, removeStrings); // reuse earlier C# version
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
                    char.IsLetter(code[i - 1]) && char.IsLetter(code[i + 1]))
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

                string codeSample = File.ReadAllText(_filename, fileEncoding);
                var tokens = TokenizeCode(codeSample, removeCodeComments, removeCodeStrings);
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
            string codeSample = File.ReadAllText(_filename, fileEncoding);
            var tokens = TokenizeCode(codeSample, removeCodeComments, removeCodeStrings);
            var words = tokens.ToArray();
            return words;
        }
        
        public override int GetWordsCount() => TokenizeCode(File.ReadAllText(_filename, fileEncoding), removeCodeComments, removeCodeStrings).ToArray().Length;
    }
}
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;

namespace NGramm
{
    public class CodeNaturalNgrammProcessor: NaturalNgrammProcessor
    {
        private string _codeTextorg = "";
        
        public CodeNaturalNgrammProcessor(string filename, ProgressReporter reporter, string textToProcess) 
            : base(filename, reporter, textToProcess) { }
        
        public override async Task PreprocessAsync()
        {
            await base.PreprocessAsync();

            _codeTextorg = readTextToProcess.Trim(); //.Replace("\r", "");
        }
        
        public override List<NGrammContainer> ProcessWordNGrammsInWindow(string[] words ,int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();
            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var wrds = words.Skip(pos).Take(windowSize).ToArray();
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessWordNgrmmToContainer(wrds, nn, false, false,  0)).ToList(), n);
                res.Add(cts);
                
                pos += windowStep;
            }
            return res;
        }
        
        public override Task ProcessWordNGramms(int n)
        {
            ignorePunctuation = false;
            ignoreCase = false;
            return base.ProcessWordNGramms(n);
        }

        public override string[] Words() => TokenizerUtils.TokenizeCode(_codeTextorg).ToArray();
        
        public override NGrammContainer IntermediateProcessWordNgrmmToContainer(
            NGrammContainer container, string[] words, int n, bool skipss, bool ignoreCase, int progressMul)
        {
            skipss = false;
            ignoreCase = false;
            
            var currentProcessorWords = Words();
            words = TokenizerUtils.WordsIntersection(words, currentProcessorWords);

            
            
            bool breaked;
            int ct = 1;

            for (int i = 0; i < words.Length; i++)
            {
                if (progressMul != 0 && i > progressMul * ct)
                {
                    MyProgressReporter?.MoveProgress();
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

                        if (ngramBuilder.Length == 0)
                        {
                            ngramBuilder.Append(word);
                        }
                        else
                        {
                            ngramBuilder.Append($" {word}");
                        }
                    }
                    
                    if (!breaked)
                    {
                        var ngram = ngramBuilder.ToString();
                        ngramBuilder.Clear();
                        container.Add(ngram);
                    }
                }
            }

            return container;
        }
    }
}
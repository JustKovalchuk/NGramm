using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO;

namespace NGramm
{
    public class CommentNgramProcessor : NaturalNgrammProcessor
    {
        public CommentNgramProcessor(string filename, ProgressReporter reporter, string textToProcess): base(filename, reporter, textToProcess) { }
    }
    public class StringNgramProcessor : NaturalNgrammProcessor
    {
        public StringNgramProcessor(string filename, ProgressReporter reporter, string textToProcess): base(filename, reporter, textToProcess) { }
    }
    
    public class ComplexNgrammProcessor : BasicNgrammProcessor
    {
        // private string tempDirName = "temp";
        
        public static bool removeCodeComments;
        public static bool removeCodeStrings;
        
        private CommentDelimiters _delimiter;
        private bool canRemoveComments = true;
        
        public bool CanRemoveComments => canRemoveComments;
        
        private HashSet<char> endsigns = new HashSet<char>(".?!;。？！¿¡؟؛¿¡።༼⸮〽⋯…⸰;".ToCharArray());

        public override HashSet<char> Endsigns { get => endsigns; set => endsigns = value; } 
        
        private NaturalNgrammProcessor _fullTextProcessor;
        private List<BasicNgrammProcessor> processors = new List<BasicNgrammProcessor>();
        
        public ComplexNgrammProcessor(string filename, ProgressReporter reporter, string textToProcess, bool removeComments, bool removeStrings): 
            base(filename, reporter, textToProcess)
        {
            removeCodeComments = removeComments;
            removeCodeStrings = removeStrings;

            _fullTextProcessor = new NaturalNgrammProcessor(filename, null, textToProcess);
            
            Utils.GetCommentsByExtension(Filename, out CommentDelimiters delimiter, out bool canRecognizeComments);
            canRemoveComments = canRecognizeComments;
            _delimiter = delimiter;
            
            Console.WriteLine("modified by LiberMaeotis creators (GDG 2025)");
        }
        
        public override async Task PreprocessAsync()
        {
            var text = readTextToProcess.Trim().Replace("\r", "");
            List<CodeBlock> codeBlocks = TokenizerUtils.ParseCodeSegments(text, _delimiter);
            codeBlocks = TokenizerUtils.CleanBlocks(codeBlocks);

            int i =  1;
            // Directory.CreateDirectory(tempDirName);
            // Utils.ClearDirectoryFiles(tempDirName);
            foreach (var codeBlock in codeBlocks)
            {
                // string extension = ".txt";
                // if (codeBlock.Type == CodeBlockType.CodeText)
                // { 
                //     extension = Path.GetExtension(Filename);
                // }
                //
                // string textPieceFilename = $"temp/{i}_output_{codeBlock.Type.ToString()}{extension}";
                //
                // File.WriteAllText(textPieceFilename, codeBlock.Content);

                if (codeBlock.Type == CodeBlockType.CodeText)
                    processors.Add(new CodeNaturalNgrammProcessor(Filename, null, codeBlock.Content));
                else if (codeBlock.Type == CodeBlockType.CommentText)
                    processors.Add(new CommentNgramProcessor(Filename, null, codeBlock.Content));
                else if (codeBlock.Type == CodeBlockType.StringText)
                    processors.Add(new StringNgramProcessor(Filename, null, codeBlock.Content));
                else
                    processors.Add(new NaturalNgrammProcessor(Filename, null, codeBlock.Content));
                i++;
            }
            
            int processorStep = 50;
            if (processors.Count > 0)
                processorStep = 50 / processors.Count;
            
            MyProgressReporter?.StartNewOperation("Ініціалізація");
            
            await _fullTextProcessor.PreprocessAsync();
            MyProgressReporter?.MoveProgress(40);
            
            foreach (var processor in processors)
            {
                await processor.PreprocessAsync();
                MyProgressReporter?.MoveProgress(processorStep);
            }
            
            MyProgressReporter?.Finish();
        }
        
        public override IReadOnlyCollection<NGrammContainer> GetLiteralNgrams() => _fullTextProcessor.GetLiteralNgrams();
        public override IReadOnlyCollection<NGrammContainer> GetSymbolNgrams() => _fullTextProcessor.GetSymbolNgrams();

        public override async Task ProcessLiteralNGramms(int n) => await _fullTextProcessor.ProcessLiteralNGramms(n);
        public override async Task ProcessSymbolNGramms(int n) => await _fullTextProcessor.ProcessSymbolNGramms(n);
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
                    var ct = ProcessWordNgrmmToContainer(words, nn, true, true, progressMult);
                    words_ngrams.Add(ct);
                });
            
                words_ngrams = new ConcurrentBag<NGrammContainer>(words_ngrams.OrderByDescending(w => w.n));
                MyProgressReporter?.Finish();
            });
        }
        
        public override List<NGrammContainer> ProcessLiteralNGrammsInWindow(
            int n, int windowSize, int windowStep, int startPos,int endPos) => 
            _fullTextProcessor.ProcessLiteralNGrammsInWindow(n, windowSize, windowStep, startPos, endPos);
        
        public override List<NGrammContainer> ProcessSymbolNGrammsInWindow(
            int n, int windowSize, int windowStep, int startPos, int endPos) => 
            _fullTextProcessor.ProcessSymbolNGrammsInWindow(n, windowSize, windowStep, startPos, endPos);

        public override List<NGrammContainer> ProcessWordNGrammsInWindow(
            string[] words, int n, int windowSize, int windowStep, int startPos, int endPos)
        {
            var res = new List<NGrammContainer>();
            int pos = startPos;
            while (endPos >= pos + windowSize)
            {
                var wrds = words.Skip(pos).Take(windowSize).ToArray();
                var cts = new NGrammContainer(Enumerable.Range(1, n).Select(nn => ProcessWordNgrmmToContainer(wrds, nn, false, false)).ToList(), n);
                res.Add(cts);
                
                pos += windowStep;
            }
            return res;
        }
        
        public override NGrammContainer IntermediateProcessWordNgrmmToContainer(
            NGrammContainer container, string[] words, int n, bool skipss, bool ignoreCase, int progressMul)
        {
            throw new NotImplementedException();
        }
        
        protected override NGrammContainer ProcessWordNgrmmToContainer(string[] words, int n, bool skipss, bool ignoreCase, int progressMul = 0)
        {
            var container = new NGrammContainer(n);

            foreach (var processor in processors)
            {
                if (processor is CommentNgramProcessor && removeCodeComments)
                    continue;
                if (processor is StringNgramProcessor && removeCodeStrings)
                    continue;
                
                container = processor.IntermediateProcessWordNgrmmToContainer(container, words, n, skipss, ignoreCase, progressMul);
            }
            
            container.Process();
            return container;
        }


        public override int GetLiteralCount(bool countSpaces) => _fullTextProcessor.GetLiteralCount(countSpaces);
        public override int GetSymbolsCount(bool countSpaces) => _fullTextProcessor.GetSymbolsCount(countSpaces);

        public override string[] Words()
        {
            string[] result = Array.Empty<string>();
            // Directory.CreateDirectory(tempDirName);
            
            foreach (var processor in processors)
            {
                if (processor is CommentNgramProcessor && removeCodeComments)
                    continue;
                if (processor is StringNgramProcessor && removeCodeStrings)
                    continue;
                
                var temp = processor.Words();
                result = result.Concat(temp).ToArray();

                // string textPieceFilename = processor.Filename;
                //
                // textPieceFilename = textPieceFilename.Replace("output", "words");
                // textPieceFilename = Path.ChangeExtension(textPieceFilename, ".txt");
                // try
                // {
                //     if (!File.Exists(textPieceFilename))
                //     {
                //         string tempText = string.Join(" ", temp);
                //
                //         File.WriteAllText(textPieceFilename, tempText);
                //     }
                // }
                // catch
                // {
                //     Console.WriteLine("Error saving debug file!");
                // }
            }

            return result;
        }
        
        public override string GetLiteralSubstring(int startPos, int length) => 
            _fullTextProcessor.GetLiteralSubstring(startPos, length);
        public override string GetSymbolsSubstring(int startPos, int length) => 
            _fullTextProcessor.GetSymbolsSubstring(startPos, length);
    }
}
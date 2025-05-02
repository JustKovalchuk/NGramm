using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;

// 1. Гісп чи працює (substring?)
// 2. Чи об'єднює як треба n-грами
// 3. ProgressBar?

namespace NGramm
{
    public enum TextType
    {
        NaturalText,
        CodeText
    }

    public class TextPieceToProcess
    {
        public string Text { get; set; }
        public TextType TextType { get; set; }
        public bool IsComment { get; set; } = false;
        public bool IsString { get; set; } = false;
    }

    public class CommentNgramProcessor : NaturalNgrammProcessor
    {
        public CommentNgramProcessor(string filename, ProgressReporter reporter): base(filename, reporter) { }
    }
    public class StringNgramProcessor : NaturalNgrammProcessor
    {
        public StringNgramProcessor(string filename, ProgressReporter reporter): base(filename, reporter) { }
    }
    
    public class ComplexNgrammProcessor : BasicNgrammProcessor
    {
        private string tempDirName = "temp";
        
        public static bool removeCodeComments;
        public static bool removeCodeStrings;
        
        private CommentDelimiters _delimiter;
        private bool canRemoveComments = true;
        
        public bool CanRemoveComments => canRemoveComments;
        
        private HashSet<char> endsigns = new HashSet<char>(".?!;。？！¿¡؟؛¿¡።༼⸮〽⋯…⸰;".ToCharArray());

        public override HashSet<char> Endsigns { get => endsigns; set => endsigns = value; } 
        
        private NaturalNgrammProcessor _fullTextProcessor;
        private List<BasicNgrammProcessor> processors = new List<BasicNgrammProcessor>();
        
        public ComplexNgrammProcessor(string filename, ProgressReporter reporter, bool removeComments, bool removeStrings): 
            base(filename, reporter)
        {
            removeCodeComments = removeComments;
            removeCodeStrings = removeStrings;

            _fullTextProcessor = new NaturalNgrammProcessor(filename, null);
            
            Utils.GetCommentsByExtension(Filename, out CommentDelimiters delimiter, out bool canRecognizeComments);
            canRemoveComments = canRecognizeComments;
            _delimiter = delimiter;
        }
        
        public override async Task PreprocessAsync()
        {
            var text = File.ReadAllText(Filename).Trim().Replace("\r", "");
            text = TokenizerUtils.RemoveStrings(text, _delimiter, out List<string> strings, false);
            text = TokenizerUtils.RemoveComments(text, _delimiter, out List<string> comments, false);
            
            List<TextPieceToProcess> textPieces = new List<TextPieceToProcess>
            {
                new TextPieceToProcess {Text=text, TextType=TextType.CodeText }
            };
            
            foreach (var strText in strings)
            {
                textPieces.Add(new TextPieceToProcess { Text=strText, TextType=TextType.NaturalText, IsString=true });
            }
            foreach (var commentText in comments)
            {
                textPieces.Add(new TextPieceToProcess { Text=commentText, TextType=TextType.NaturalText, IsComment=true });
            }

            int i =  1;
            Directory.CreateDirectory(tempDirName);
            Utils.ClearDirectoryFiles(tempDirName);
            foreach (var textPiece in textPieces)
            {
                string extension = ".txt";
                if (textPiece.TextType == TextType.CodeText)
                { 
                    extension = Path.GetExtension(Filename);
                }
                
                string textPieceFilename = $"temp/{i}_output_{textPiece.TextType.ToString()}{extension}";
                if (textPiece.TextType == TextType.NaturalText)
                {
                    if (textPiece.IsComment)
                        textPieceFilename = $"temp/{i}_output_comment_{textPiece.TextType.ToString()}{extension}";
                    else if (textPiece.IsString)
                        textPieceFilename = $"temp/{i}_output_string_{textPiece.TextType.ToString()}{extension}";
                    else
                        textPieceFilename = $"temp/{i}_output_unknown_{textPiece.TextType.ToString()}{extension}";
                }
                
                File.WriteAllText(textPieceFilename, textPiece.Text);
                Console.WriteLine("Wrote file!" + textPieceFilename);

                if (textPiece.TextType == TextType.CodeText)
                {
                    processors.Add(new CodeNaturalNgrammProcessor(textPieceFilename, null));
                }
                else if (textPiece.TextType == TextType.NaturalText)
                {
                    if (textPiece.IsComment)
                        processors.Add(new CommentNgramProcessor(textPieceFilename, null));
                    else if (textPiece.IsString)
                        processors.Add(new StringNgramProcessor(textPieceFilename, null));
                    else
                        processors.Add(new NaturalNgrammProcessor(textPieceFilename, null));
                }
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
                MyProgressReporter?.StartNewOperation($"Обчислення словесних н-грамм від 1 до {n}");
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
            Directory.CreateDirectory(tempDirName);
            
            foreach (var processor in processors)
            {
                if (processor is CommentNgramProcessor && removeCodeComments)
                    continue;
                if (processor is StringNgramProcessor && removeCodeStrings)
                    continue;
                
                var temp = processor.Words();
                result = result.Concat(temp).ToArray();

                string textPieceFilename = processor.Filename;
                
                textPieceFilename = textPieceFilename.Replace("output", "words");
                textPieceFilename = Path.ChangeExtension(textPieceFilename, ".txt");
                try
                {
                    if (!File.Exists(textPieceFilename))
                    {
                        string tempText = string.Join(" ", temp);

                        Console.WriteLine(textPieceFilename);

                        File.WriteAllText(textPieceFilename, tempText);
                    }
                }
                catch
                {
                    Console.WriteLine("Error saving debug file!");
                }
                
            }

            return result;
        }
        
        public override string GetLiteralSubstring(int startPos, int length) => 
            _fullTextProcessor.GetLiteralSubstring(startPos, length);
        public override string GetSymbolsSubstring(int startPos, int length) => 
            _fullTextProcessor.GetSymbolsSubstring(startPos, length);
    }
}
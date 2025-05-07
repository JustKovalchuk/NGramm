using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;

namespace NGramm
{
    public abstract class BasicNgrammProcessor
    {
        public int CountDesiredVariables = 0;
        
        public ProgressReporter MyProgressReporter;
        
        protected readonly string readTextToProcess;
        public readonly string Filename;
        protected readonly Encoding FileEncoding;
        
        protected ConcurrentBag<NGrammContainer> literal_ngrams = new ConcurrentBag<NGrammContainer>();
        protected ConcurrentBag<NGrammContainer> symbol_ngrams = new ConcurrentBag<NGrammContainer>();
        protected ConcurrentBag<NGrammContainer> words_ngrams = new ConcurrentBag<NGrammContainer>();
        
        public static bool ignorePunctuation = true;
        public static bool ignoreCase = true;
        public static bool process_spaces = true;
        
        public abstract HashSet<char> Endsigns { get; set; }

        protected BasicNgrammProcessor(string filename, ProgressReporter reporter, string textToProcess)
        {
            Filename = filename;
            FileEncoding = Utils.GetEncoding(filename);
            readTextToProcess = textToProcess;
            MyProgressReporter = reporter;
            CountDesiredVariables = 0;
        }
        
        protected void ClearAllNGrammContainers()
        {
            symbol_ngrams = new ConcurrentBag<NGrammContainer>();
            literal_ngrams = new ConcurrentBag<NGrammContainer>();
            words_ngrams = new ConcurrentBag<NGrammContainer>();
        }
        
        public abstract Task PreprocessAsync();
        
        public virtual IReadOnlyCollection<NGrammContainer> GetLiteralNgrams() => literal_ngrams;

        public virtual IReadOnlyCollection<NGrammContainer> GetSymbolNgrams() => symbol_ngrams;

        public virtual IReadOnlyCollection<NGrammContainer> GetWordsNgrams() => words_ngrams;

        public abstract Task ProcessLiteralNGramms(int n);
        public abstract Task ProcessSymbolNGramms(int n);
        public abstract Task ProcessWordNGramms(int n);

        public abstract List<NGrammContainer> ProcessLiteralNGrammsInWindow(
            int n, int windowSize, int windowStep, int startPos,int endPos);
        public abstract List<NGrammContainer> ProcessSymbolNGrammsInWindow(
            int n, int windowSize, int windowStep, int startPos, int endPos);
        public abstract List<NGrammContainer> ProcessWordNGrammsInWindow(
            string[] words, int n, int windowSize, int windowStep, int startPos, int endPos);


        public abstract NGrammContainer IntermediateProcessWordNgrmmToContainer(
            NGrammContainer container, string[] words, int n, bool skipss, bool ignoreCase, int progressMul);
        protected abstract NGrammContainer ProcessWordNgrmmToContainer(
            string[] words, int n, bool skipss, bool ignoreCase, int progressMul = 0);
        
        public abstract int GetLiteralCount(bool countSpaces);        
        public abstract int GetSymbolsCount(bool countSpaces);

        public abstract string[] Words();
        public int GetWordsCount() => Words().Length;

        public abstract string GetLiteralSubstring(int startPos, int length);
        public abstract string GetSymbolsSubstring(int startPos, int length);

    }
}
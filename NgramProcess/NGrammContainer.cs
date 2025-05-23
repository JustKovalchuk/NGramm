﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NGramm
{
    public class NGrammContainer
    {
        public int n;
        public Dictionary<string, NGramm> ngrams = new Dictionary<string, NGramm>();
        public Dictionary<int, List<NGramm>> ngram_reps = new Dictionary<int, List<NGramm>>();
        public Dictionary<string, int> ngram_reps_i = new Dictionary<string, int>();
        public int count = 0;
        public int absCount = 0;

        public NGrammContainer(int n)
        {
            this.n = n;
        }

        public NGrammContainer(List<NGrammContainer> containers, int n = 0)
        {
            this.n = n;
            foreach (NGrammContainer cont in containers)
            {
                foreach (string key in cont.ngrams.Keys)
                {
                    Add(key, cont.ngrams[key].count);
                }
            }
            Process();
        }

        public void Add(string ngram)
        {
            count++;
            if (ngrams.ContainsKey(ngram))
            {
                ngrams[ngram].count++;
            }
            else
            {
                ngrams.Add(ngram, new NGramm());
                ngrams[ngram].text = ngram;
                ngrams[ngram].count = 1;
                absCount++;
            }
        }
        
        public void Add(string ngram, int ngramCount)
        {
            count++;
            if (ngrams.ContainsKey(ngram))
            {
                ngrams[ngram].count += ngramCount;
            }
            else
            {
                ngrams.Add(ngram, new NGramm());
                ngrams[ngram].text = ngram;
                ngrams[ngram].count = ngramCount;
                absCount += ngramCount;
            }
        }

        public void Process()
        {
            ngrams = ngrams.AsParallel().OrderByDescending(x => x.Value.count).ToDictionary(x => x.Key, x => x.Value);
            foreach (NGramm ng in ngrams.Values)
            {
                ng.f = (float)ng.count / count;

                if (ngram_reps.ContainsKey(ng.count))
                {
                    ngram_reps[ng.count].Add(ng);
                    ngram_reps_i[ng.count.ToString()]++;
                }
                else
                {
                    ngram_reps.Add(ng.count, new List<NGramm>() { ng });
                    ngram_reps_i.Add(ng.count.ToString(), 1);
                }

            }
            //Dictionary<int, int> sorted = new Dictionary<int, int>();
            //ngram_reps_i = Helpers.SortByVal(ngram_reps_i);
        }

        public Dictionary<string, int> GetNgrams(int countFilter = 0)
        {
            return ngrams.
                Where(n => n.Value.count >= countFilter).
                ToDictionary(n => n.Key, n => n.Value.count);
        }
    }
}

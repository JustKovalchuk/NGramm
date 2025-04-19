using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGramm
{
    public class Statistics
    {
        private Dictionary<int, double> zipf1Stats = new Dictionary<int, double>();
        private Dictionary<int, double> zipf1StatsL = new Dictionary<int, double>();
        private Dictionary<double, double> zipf2Stats = new Dictionary<double, double>();
        private Dictionary<double, double> paretoStats = new Dictionary<double, double>();
        private Dictionary<double, double> paretoStats2 = new Dictionary<double, double>();
        private Dictionary<int, double> hips = new Dictionary<int, double>();
        public Dictionary<int, double> hips_d = new Dictionary<int, double>();
        
        public Statistics(NGrammContainer container,bool CommonRank)
        {
            int i = 1;
            int g = 1;
            int total = 0;
            foreach (List<NGramm> reps in container.ngram_reps.Values)
            {
                total += reps.Count;
            }
            foreach (int reps in container.ngram_reps.Keys)
            {
                foreach (NGramm rp in container.ngram_reps[reps])
                {
                    if (zipf1StatsL.Values.Contains(rp.count)&& CommonRank)
                    {
                        continue;
                    }
                    else
                    {
                        zipf1StatsL.Add(g, rp.count);
                        g++;
                    }
                   
                }
                zipf1Stats.Add(i, reps); //delete this if not neded
                i++;
            }
            List<int> tmp = container.ngram_reps.Keys.ToList();
            tmp.Sort();
            tmp.Reverse();
            foreach (int reps in tmp)
            {
                zipf2Stats.Add(reps, container.ngram_reps[reps].Count/(double)total);
            }
          
            i = 1;
            List<int> a = new List<int>();
            foreach (var item in container.ngram_reps.Values)
            {
                a.Add(item.Count);
            }
            a.Reverse();
            foreach (int reps in a)
            {
                paretoStats.Add(i, reps / (double)total);
                i++;
            }
            paretoStats.Reverse();
            List<double> v = zipf2Stats.Keys.ToList();
            v.Reverse();
            foreach (int rank in paretoStats.Keys)
            {
                double par = 0;
                for (i = rank-1; i < paretoStats.Count; i++)
                {
                    par += paretoStats.Values.ToArray()[i];
                }
                paretoStats2.Add(v[rank-1], par);
            }
        }

        public Dictionary<int, double> GetZipf1Stats()
        {
            return zipf1Stats;
        }

        public Dictionary<int, double> GetZipf1StatsL()
        {
            return zipf1StatsL;
        }

        public Dictionary<double, double> GetZipf2Stats()
        {
            return zipf2Stats;
        }

        public Dictionary<double, double> GetParetroStats()
        {
            return paretoStats2;
        }

        public Dictionary<int, double> GetHipsStats()
        {
            return hips;
        }
    }
}

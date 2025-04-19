using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGramm
{
    internal static class PerformanceSettings
    {
        public static int MaxCores = Environment.ProcessorCount - 1;
        public static int Cores = MaxCores;

        public static int MinNGrammCount = 1;

        public static ParallelOptions ParallelOpt => new ParallelOptions
        {
            MaxDegreeOfParallelism = Cores
        };
    }
}

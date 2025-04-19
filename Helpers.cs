using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NGramm
{
    public static class Helpers
    {
        public static Dictionary<string, int> SortByVal(Dictionary<string, int> _strings)
        {

            return _strings.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<double, double> SortByVal(Dictionary<double, double> _strings)
        {

            return _strings.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<int, int> SortByVal(Dictionary<int, int> _strings)
        {
            return _strings.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<int, int> SortByKey(Dictionary<int, int> _dict)
        {
            return _dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }

        public static Dictionary<double, double> SortByKey(Dictionary<double, double> _dict)
        {
            return _dict.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}

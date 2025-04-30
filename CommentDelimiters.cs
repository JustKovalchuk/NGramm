using System;
using System.Collections.Generic;

namespace NGramm
{
    public class CommentDelimiters
    {
        public List<Tuple<string, string>> MultiLine { get; set; }
        public List<string> SingleLine { get; set; }

        public CommentDelimiters()
        {
            MultiLine = new List<Tuple<string, string>>();
            SingleLine = new List<string>();
        }
    }
}
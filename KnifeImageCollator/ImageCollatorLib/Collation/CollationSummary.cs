using System;
using System.Collections.Generic;

namespace ImageCollatorLib
{
    public class CollationSummary
    {
        public int Summaries = 0;
        public int Files = 0;
        public List<string> Errors = new List<string>();
        public List<Exception> Exceptions = new List<Exception>();
    }
}

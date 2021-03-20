using System;
using System.Collections.Generic;

namespace ImageCollatorFunction
{
    public class ImageCollatorOutputs
    {
        public int summaries { get; set; } = 0;
        public int files { get; set; } = 0;
        public List<string> errors { get; set; } = new List<string>();
    }
}

using System;
namespace ImageCollatorFunction
{
    public class ImageCollatorInputs
    {
        public string collation { get; set; }
        public string[] accounts { get; set; }
        public string period { get; set; }
        public string group { get; set; }
        public string filter { get; set; }
        public string[] keywords_list { get; set; }
        public string keywords_list_url { get; set; }
    }
}

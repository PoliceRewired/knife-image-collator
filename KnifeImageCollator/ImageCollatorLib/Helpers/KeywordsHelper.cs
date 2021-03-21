using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ImageCollatorLib.Helpers
{
    public class KeywordsHelper
    {
        public static async Task<IEnumerable<string>> FindKeywordsAsync(IEnumerable<string> keywordsList, string keywordsListUrl)
        {
            var list = new List<string>();
            if (!string.IsNullOrWhiteSpace(keywordsListUrl))
            {
                using (var web = new WebClient())
                {
                    var keywordsData = await web.DownloadStringTaskAsync(new Uri(keywordsListUrl));
                    list.AddRange(keywordsData.Split('\n').Select(k => k.Trim().ToLower()));
                }
            }
            if (keywordsList != null && keywordsList.Count() > 0)
            {
                list.AddRange(keywordsList.Select(k => k.Trim().ToLower()));
            }
            return list;
        }
    }
}

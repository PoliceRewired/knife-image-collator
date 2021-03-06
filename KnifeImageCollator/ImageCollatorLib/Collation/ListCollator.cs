using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Collation
{
    public class ListCollator : AbstractCollator
    {
        public ListCollator(string group, Action<string> log) : base(group, log)
        {
        }

        protected override async Task<IEnumerable<MediaDetails>> ReadCurrentCsvAsync(string path)
        {
            return new List<MediaDetails>();
        }

        protected override async Task StoreNewCsvAsync(IEnumerable<MediaDetails> medias, string path)
        {
            Log("CSV path: " + path);
            foreach (var media in medias)
            {
                Log(string.Format("Image {0} from: {1}", media.MediaIndex, media.TweetText));
            }
        }

        protected override async Task CommitTransactionAsync() { }
        protected override async Task InitTransactionAsync() { }
        protected override async Task TransferImageAsync(string url, string path) { }
    }
}

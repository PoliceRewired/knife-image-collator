using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Collation
{
    public abstract class AbstractCollator : ICollator
    {
        public static readonly string DEFAULT_CSV_FILENAME = "collated-media.csv";

        private string group;
        private string csvFilename;
        private Action<string> log;

        protected AbstractCollator(string group, Action<string> log)
        {
            this.csvFilename = DEFAULT_CSV_FILENAME;
            this.group = group;
            this.log = log;
        }

        public string Group => group;
        public string CsvFilename => csvFilename;

        public async Task<CollationSummary> CollateAsync(IEnumerable<MediaDetails> medias)
        {
            var summary = new CollationSummary();
            await InitTransactionAsync();

            try
            {
                Log(string.Format("Summarising {0} media...", medias.Count()));
                await AppendCsvAsync(medias, Path.Combine(GroupPath, CsvFilename));
                summary.Summaries += medias.Count();
            }
            catch (Exception e)
            {
                var msg = "Failed to summarise tweets: " + e.Message;
                Log(msg);
                summary.Errors.Add(msg);
            }

            Log(string.Format("Downloading {0} media...", medias.Count()));
            foreach (var media in medias)
            {
                int count = 0;
                try
                {
                    int mediaIndex = count++;
                    var mediaPath = Path.Combine(GroupPath, MediaDirectory(media), media.Filename);
                    Log("Retrieving: " + media.MediaUrl);
                    await TransferImageAsync(media.MediaUrl, mediaPath);
                    Log("Stored at:  " + mediaPath);
                    summary.Files++;
                }
                catch (Exception e)
                {
                    var msg = "Failed to transfer image: " + e.Message;
                    Log(msg);
                    summary.Errors.Add(msg);
                }
            } // each tweet

            await CommitTransactionAsync();
            return summary;
        }

        protected abstract Task AppendCsvAsync(IEnumerable<MediaDetails> medias, string path);
        protected abstract Task TransferImageAsync(string url, string path);
        protected abstract Task InitTransactionAsync();
        protected abstract Task CommitTransactionAsync();

        protected void Log(string message) { log(message); }

        protected string GroupPath => group;

        protected string MediaDirectory(MediaDetails media) => media.Created.ToString("yyyy-MM-dd");

    }
}

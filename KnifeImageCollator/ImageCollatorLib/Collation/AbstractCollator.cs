using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public bool Verbose { get; set; }

        public string Group => group;
        public string CsvFilename => csvFilename;

        public async Task<CollationSummary> CollateAsync(IEnumerable<MediaDetails> medias)
        {
            var summary = new CollationSummary();
            try
            {
                await InitTransactionAsync();
                Log(string.Format("Summarising {0} media...", medias.Count()));

                // retrieve
                var csvPath = Path.Combine(GroupPath, CsvFilename);
                var storedMedias = await ReadCurrentCsvAsync(csvPath);

                // append non-duplicates
                var newMedias = storedMedias.ToList();
                newMedias.AddRange(medias.Where(m => !storedMedias.Any(sm => sm.TweetId == m.TweetId && sm.MediaId == m.MediaId)));

                // store
                await StoreNewCsvAsync(medias, csvPath);
                summary.Summaries += medias.Count();

                // move on to individual images
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
                        var msg = string.Format("Failed to transfer image, with {0}: {1}", e.GetType().Name, e.Message);
                        Log(msg); if (Verbose) { Log(e.StackTrace); }
                        summary.Errors.Add(msg);
                        summary.Exceptions.Add(e);
                    }
                } // each tweet
            }
            catch (Exception e)
            {
                var msg = string.Format("Failed to summarise tweets, with {0}: {1}", e.GetType().Name, e.Message);
                Log(msg); if (Verbose) { Log(e.StackTrace); }
                summary.Errors.Add(msg);
                summary.Exceptions.Add(e);
            }
            finally
            {
                try
                {
                    await CommitTransactionAsync();
                }
                catch (Exception ee)
                {
                    var msg = string.Format("Failed to commit transaction, with {0}: {1}", ee.GetType().Name, ee.Message);
                    Log(msg); if (Verbose) { Log(ee.StackTrace); }
                    summary.Errors.Add(msg);
                    summary.Exceptions.Add(ee);
                }
            }
            return summary;
        }

        protected abstract Task<IEnumerable<MediaDetails>> ReadCurrentCsvAsync(string path);
        protected abstract Task StoreNewCsvAsync(IEnumerable<MediaDetails> medias, string path);
        protected abstract Task TransferImageAsync(string url, string path);
        protected abstract Task InitTransactionAsync();
        protected abstract Task CommitTransactionAsync();

        protected void Log(string message) { log(message); }

        protected string GroupPath => group;

        // protected string MediaDirectory(MediaDetails media) => media.Created.ToString("yyyy-MM-dd");
        protected string MediaDirectory(MediaDetails media) => media.Created.ToString("yyyy-MM");
    }
}

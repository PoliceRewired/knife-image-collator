using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CsvHelper;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;

namespace ImageCollatorLib.Collation
{
    public class DownloadCollator : AbstractCollator
    {
        public DownloadCollator(string group, Action<string> log) : base(group, log)
        {
        }

        protected override async Task InitTransactionAsync() { }

        protected override async Task<IEnumerable<MediaDetails>> ReadCurrentCsvAsync(string path)
        {
            if (File.Exists(path))
            {
                using (var stream = File.OpenRead(path))
                {
                    return CsvFileHelper.ReadCsv(stream);
                }
            }
            else
            {
                return new List<MediaDetails>();
            }
        }

        protected override async Task StoreNewCsvAsync(IEnumerable<MediaDetails> medias, string path)
        {
            // ensure the directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            CsvFileHelper.AppendCsvFile(path, medias);
        }

        protected override async Task TransferImageAsync(string url, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(new Uri(url), path);
            }
        }

        protected override async Task CommitTransactionAsync() { }
    }
}

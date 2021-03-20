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

namespace ImageCollatorLib.Collation
{
    public class DownloadCollator : AbstractCollator
    {
        public DownloadCollator(string group, Action<string> log) : base(group, log)
        {
        }

        protected override async Task InitTransactionAsync() { }

        protected override async Task AppendCsvAsync(IEnumerable<MediaDetails> medias, string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            Helpers.CsvHelper.AppendCsvFile(path, medias);
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

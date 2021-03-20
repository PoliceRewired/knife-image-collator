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
    public class S3Collator : AbstractCollator
    {
        string bucket;
        S3Helper s3;

        public S3Collator(string group, string bucket, Action<string> log) : base(group, log)
        {
            this.bucket = bucket;
        }

        protected override async Task InitTransactionAsync()
        {
            s3 = s3 ?? new S3Helper(bucket, Log);
        }

        protected override async Task CommitTransactionAsync()
        {
            s3.Dispose();
            s3 = null;
        }

        protected override async Task<IEnumerable<MediaDetails>> ReadCurrentCsvAsync(string path)
        {
            var keys = await s3.ListBucketObjects();

            if (keys.Contains(path))
            {
                using (var csvStream = await s3.GetObjectAsync(path))
                {
                    return Helpers.CsvFileHelper.ReadCsv(csvStream);
                }
            }
            else
            {
                return new List<MediaDetails>();
            }
        }

        protected override async Task StoreNewCsvAsync(IEnumerable<MediaDetails> medias, string path)
        {
            await s3.StreamCsvToS3Async(medias, path);
        }

        protected override async Task TransferImageAsync(string url, string path)
        {
            await s3.TransferFileToS3Async(url, path);
        }
    }
}

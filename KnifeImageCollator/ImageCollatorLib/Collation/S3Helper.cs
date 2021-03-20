using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using CsvHelper;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib
{
    public class S3Helper : IDisposable
    {
        public AmazonS3Client Client { get; private set; }
        public string Bucket { get; private set; }
        private Action<string> log;

        public S3Helper(string bucket, Action<string> log, RegionEndpoint region = null)
        {
            this.Bucket = bucket;
            this.Client = new AmazonS3Client(region ?? RegionEndpoint.EUWest2);
            this.log = log;
        }

        public void Dispose() => Client?.Dispose();

        protected void Log(string message) => log(message);

        public async Task<bool> SaveToS3Async(string key, string content)
        {
            var request = new PutObjectRequest
            {
                BucketName = Bucket,
                Key = key,
                ContentBody = content
            };
            var response = await Client.PutObjectAsync(request);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> SaveToS3Async(string key, Stream input)
        {
            var request = new PutObjectRequest
            {
                BucketName = Bucket,
                Key = key,
                InputStream = input
            };
            var response = await Client.PutObjectAsync(request);
            return response.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task<IEnumerable<string>> ListBucketObjects()
        {
            var request = new ListObjectsV2Request()
            {
                BucketName = Bucket
            };
            var objects = await Client.ListObjectsV2Async(request);
            return objects.S3Objects.Select(o => o.Key);
        }

        public async Task<Stream> GetObjectAsync(string key)
        {
            var request = new GetObjectRequest
            {
                BucketName = Bucket,
                Key = key
            };
            var result = await Client.GetObjectAsync(request);
            return result.ResponseStream;
        }

        public async Task StreamCsvToS3Async(IEnumerable<MediaDetails> medias, string fileKey)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(medias);
                    await csv.FlushAsync();
                    using (var transfer = new TransferUtility(Client))
                    {
                        output.Position = 0;
                        await transfer.UploadAsync(output, Bucket, fileKey);
                    }
                }
            }
        }

        public async Task TransferFileToS3Async(string url, string path)
        {
            var uri = new Uri(url);
            var data = new MemoryStream();
            using (var client = new WebClient())
            {
                using (var downloadStream = await client.OpenReadTaskAsync(uri))
                {
                    await downloadStream.CopyToAsync(data);
                    data.Position = 0;

                    var request = new PutObjectRequest()
                    {
                        Key = path,
                        BucketName = Bucket,
                        InputStream = data
                    };
                    var response = await Client.PutObjectAsync(request);
                    if (response.HttpStatusCode != HttpStatusCode.OK) { throw new Exception("failed"); }
                }
            }
        }
    }
}
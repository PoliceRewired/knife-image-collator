using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;

namespace ImageCollatorLib
{
    public class S3Helper : IDisposable
    {
        public AmazonS3Client Client { get; private set; }
        public string Bucket { get; private set; }
        private Action<string> Log;
        

        public S3Helper(string bucket, Action<string> log, RegionEndpoint region = null)
        {
            Bucket = bucket;
            Client = new AmazonS3Client(region ?? RegionEndpoint.EUWest2);
            Log = log;
        }

        public void Dispose()
        {
            Client.Dispose();
        }

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

    }
}
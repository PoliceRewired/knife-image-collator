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
using CsvHelper.Configuration;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib
{
    public class TweetCollator
    {
        Filter filter;
        CollateAction action;
        string group;
        string csvFilename;
        Func<ITweet, bool> tweetFilter;
        Func<IMediaEntity, bool> mediaFilter;
        Action<string> Log;
        string bucket;

        public TweetCollator(Filter filter, CollateAction action, string group, string csvFilename, Action<string> log, string bucket = null)
        {
            this.filter = filter;
            this.action = action;
            this.group = group;
            this.csvFilename = csvFilename;
            this.Log = log;
            this.bucket = bucket;

            tweetFilter = FilterHelper.ParseTweetFilter(filter);
            mediaFilter = FilterHelper.ParseMediaFilter(filter);
        }

        public static TweetCollator FromArgs(string filter, string action, string group, string csvFilename, Action<string> log)
        {
            return new TweetCollator(
                FilterHelper.EnsureArgument<Filter>(filter, "filter"),
                FilterHelper.EnsureArgument<CollateAction>(action, "action"),
                group, csvFilename, log);
        }

        public string CalcTweetDirectoryPath(MinimalTweetDTO mt)
        {
            var path = Path.Combine(group, mt.Created.ToString("yyyy-MM-dd"));
            return path;
        }

        public string CalcGroupDirectoryPath()
        {
            return group;
        }

        public string CalcMediaFilename(MinimalTweetDTO tweet, string url, int index)
        {
            if (url.Contains("."))
            {
                var parts = url.Split(".");
                var suffix = parts[parts.Length - 1];
                return string.Format("{0}-{1}-{2}-{3}.{4}", tweet.Created.Ticks, tweet.Username, tweet.TweetId, index, suffix);
            }
            else
            {
                return string.Format("{0}-{1}-{2}-{3}", tweet.Created.Ticks, tweet.Username, tweet.TweetId, index);
            }
        }

        public void EnsurePath(string path)
        {
            switch (action)
            {
                case CollateAction.download:
                    Directory.CreateDirectory(path);
                    break;
            }
        }

        public async Task CollateAsync(IEnumerable<MinimalTweetDTO> tweets)
        {
            foreach (var tweet in tweets)
            {
                Log("Text:  " + tweet.Text);
                switch (action)
                {
                    case CollateAction.list:
                        break;

                    case CollateAction.s3:
                        Log("Storing in S3...");
                        var groupKey = CalcGroupDirectoryPath();
                        var csvFileKey = Path.Combine(groupKey, csvFilename);
                        using (var s3 = new S3Helper(bucket, Log))
                        {
                            var keys = await s3.ListBucketObjects();

                            if (keys.Contains(csvFileKey))
                            {
                                var recordedTweets = new List<MinimalTweetDTO>();
                                using (var csvStream = await s3.GetObjectAsync(csvFileKey))
                                {
                                    recordedTweets = CsvHelper.ReadCsv(csvStream);
                                    recordedTweets.AddRange(tweets);
                                    await StreamToS3Async(s3, recordedTweets, csvFileKey);
                                }
                            }
                            else
                            {
                                await StreamToS3Async(s3, tweets, csvFileKey);
                            }

                            int index = 0;
                            foreach (var mediaURL in tweet.ImageUrls)
                            {
                                var tweetDirectoryKey = CalcTweetDirectoryPath(tweet);
                                int mediaIndex = index++;
                                var mediaFileName = CalcMediaFilename(tweet, mediaURL, mediaIndex);
                                var mediaKey = Path.Combine(tweetDirectoryKey, mediaFileName);

                                Log("Transferring... " + mediaURL);
                                await TransferMediaAsync(s3, mediaURL, mediaKey);
                                Log("Uploaded to s3 key: " + mediaKey);
                            }
                        }
                        break;

                    case CollateAction.download:
                        Log("Downloading...");

                        var groupPath = CalcGroupDirectoryPath();
                        EnsurePath(groupPath);
                        var csvFilePath = Path.Combine(groupPath, csvFilename);
                        CsvHelper.AppendCsvFile(csvFilePath, tweet);

                        int count = 0;
                        foreach (var mediaURL in tweet.ImageUrls)
                        {
                            var tweetDirectory = CalcTweetDirectoryPath(tweet);
                            EnsurePath(tweetDirectory);
                            int mediaIndex = count++;
                            var name = CalcMediaFilename(tweet, mediaURL, mediaIndex);
                            var path = Path.Combine(tweetDirectory, name);
                            Log("Downloading... " + mediaURL);
                            await DownloadMediaToFileAsync(mediaURL, path);
                            Log("Downloaded to: " + path);
                        }
                        break;

                    default:
                        throw new NotImplementedException("Action not implemented: " + action.ToString());
                }
            }
        }

        private async Task StreamToS3Async(S3Helper s3, IEnumerable<MinimalTweetDTO> tweets, string fileKey)
        {
            var output = new MemoryStream();
            using (var writer = new StreamWriter(output))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    await csv.WriteRecordsAsync(tweets);
                    await csv.FlushAsync();
                    using (var transfer = new TransferUtility(s3.Client))
                    {
                        output.Position = 0;
                        await transfer.UploadAsync(output, bucket, fileKey);
                    }
                }
            }

        }

        public async Task TransferMediaAsync(S3Helper s3, string urlFrom, string keyTo)
        {
            var uri = new Uri(urlFrom);
            using (var client = new WebClient())
            {
                using (var dataStream = await client.OpenReadTaskAsync(uri))
                {
                    using (var transfer = new TransferUtility(s3.Client))
                    {
                        //var request = new GetPreSignedUrlRequest()
                        //{
                        //    BucketName = bucket,
                        //    Key = keyTo
                        //};
                        //var url = s3.Client.GetPreSignedURL(request);

                        var request = new PutObjectRequest()
                        {
                            Key = keyTo,
                            BucketName = bucket,
                            InputStream = dataStream
                        };
                        var response = await s3.Client.PutObjectAsync(request);
                        if (response.HttpStatusCode != HttpStatusCode.OK) { throw new Exception("failed"); }

                        // await transfer.UploadAsync(dataStream, bucket, keyTo);
                        // TODO: clean up
                    }
                }
            }
        }

        public async Task DownloadMediaToFileAsync(string urlFrom, string pathTo)
        {
            var uri = new Uri(urlFrom);
            using (var client = new WebClient())
            {
                await client.DownloadFileTaskAsync(uri, pathTo);
            }
        }
    }
}

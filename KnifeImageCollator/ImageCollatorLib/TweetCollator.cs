using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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

        public TweetCollator(Filter filter, CollateAction action, string group, string csvFilename, Action<string> log)
        {
            this.filter = filter;
            this.action = action;
            this.group = group;
            this.csvFilename = csvFilename;
            this.Log = log;
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
                switch (action)
                {
                    case CollateAction.list:
                        Log("Text: " + tweet.Text);
                        break;

                    case CollateAction.download:
                        Log("Text:  " + tweet.Text);

                        var groupPath = CalcGroupDirectoryPath();
                        EnsurePath(groupPath);
                        var csvFilePath = Path.Combine(groupPath, csvFilename);
                        AppendCsv(csvFilePath, tweet);

                        int count = 0;
                        foreach (var mediaURL in tweet.ImageUrls)
                        {
                            var tweetDirectory = CalcTweetDirectoryPath(tweet);
                            EnsurePath(tweetDirectory);
                            int mediaIndex = count++;
                            var name = CalcMediaFilename(tweet, mediaURL, mediaIndex);
                            var path = Path.Combine(tweetDirectory, name);
                            Log("Downloading... " + mediaURL);
                            await RetrieveMediaAsync(mediaURL, path);
                            Log("Downloaded to: " + path);
                        }
                        break;

                    default:
                        throw new NotImplementedException("Action not implemented: " + action.ToString());
                }
            }
        }

        public void AppendCsv(string path, params MinimalTweetDTO[] tweets)
        {
            switch (action)
            {
                case CollateAction.list:
                    // NOP
                    break;

                case CollateAction.download:
                    if (!File.Exists(path))
                    {
                        using (var writer = new StreamWriter(path))
                        {
                            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                            {
                                csv.WriteRecords(tweets);
                            }
                        }
                    }
                    else
                    {
                        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                        {
                            HasHeaderRecord = false,
                        };
                        using (var stream = File.Open(path, FileMode.Append))
                        using (var writer = new StreamWriter(stream))
                        using (var csv = new CsvWriter(writer, config))
                        {
                            csv.WriteRecords(tweets);
                        }
                    }
                    break;

                case CollateAction.s3:
                    throw new NotImplementedException("s3 csv not implemented yet");
            }
        }

        public async Task RetrieveMediaAsync(string urlFrom, string pathTo)
        {
            var uri = new Uri(urlFrom);
            switch (action)
            {
                case CollateAction.list:
                    // NOP
                    break;

                case CollateAction.download:
                    using (var client = new WebClient())
                    {
                        await client.DownloadFileTaskAsync(uri, pathTo);
                    }
                    break;

                case CollateAction.s3:
                    throw new NotImplementedException("s3 download not implemented yet");
            }

        }
    }
}

using System;
using CsvHelper.Configuration.Attributes;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib.Entities
{
    public class MediaDetails
    {
        public DateTime Created { get; set; }
        public string CreatedUniversal => Created.ToUniversalTime().ToString("u");
        public string CreatedSimple => Created.ToString("yyyy-MM-dd hh:mm:ss");
        public string MediaUrl { get; set; }
        public long TweetId { get; set; }
        public long MediaId { get; set; }
        public int MediaIndex { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string TweetUrl { get; set; }
        public string TweetText { get; set; }

        public static MediaDetails From(ITweet tweet, IMediaEntity media, int index)
        {
            return new MediaDetails
            {
                Created = tweet.CreatedAt.DateTime,
                Username = tweet.CreatedBy.ScreenName,
                UserId = tweet.CreatedBy.Id,
                TweetId = tweet.Id,
                MediaId = media.Id.GetValueOrDefault(),
                MediaIndex = index,
                MediaUrl = media.MediaURLHttps,
                TweetUrl = tweet.Url,
                TweetText = tweet.FullText
            };
        }

        public string Filename
        {
            get
            {
                if (MediaUrl.Contains("."))
                {
                    var parts = MediaUrl.Split(".");
                    var suffix = parts[parts.Length - 1];
                    return string.Format("{0} {1} {2} {3}.{4}",
                        CreatedSimple,
                        Username,
                        TweetId,
                        MediaIndex,
                        suffix);
                }
                else
                {
                    return string.Format("{0} {1} {2} {3}",
                        CreatedSimple,
                        Username,
                        TweetId,
                        MediaIndex);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib
{
    public class TwitterInspector
    {
        protected TwitterClient client;

        public TwitterInspector(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Action<string> logger)
        {
            this.client = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            this.Log = logger;
        }

        protected Action<string> Log;

        public async Task<IEnumerable<MinimalTweetDTO>> FilterTimelineAsync(string username, DateTime earliestDate, DateTime latestDate, Func<ITweet,bool> tweetFilter, Func<IMediaEntity,bool> mediaFilter)
        {
            // compose search criteria
            var criteria = string.Format("from:{0} since:{1} until:{2}",
                username,
                earliestDate.ToString("yyyy-MM-dd"),
                latestDate.ToString("yyyy-MM-dd"));

            Log(string.Format("Search criteria: {0}", criteria));

            // search
            var collatedTweets = new List<MinimalTweetDTO>();
            var searchIterator = client.Search.GetSearchTweetsIterator(criteria);
            while (!searchIterator.Completed)
            {
                var searchPage = await searchIterator.NextPageAsync();
                foreach (var tweet in searchPage)
                {
                    if (tweetFilter(tweet))
                    {
                        var minimalTweet = new MinimalTweetDTO()
                        {
                            Created = tweet.CreatedAt.DateTime,
                            Text = tweet.FullText,
                            Username = tweet.CreatedBy.ScreenName,
                            UserId = tweet.CreatedBy.Id,
                            TweetId = tweet.Id,
                            ImageUrls = tweet.Media.Where(mediaFilter).Select(m => m.MediaURLHttps)
                        };
                        collatedTweets.Add(minimalTweet);
                    }                    
                }
            }

            return collatedTweets;
        }

        public static bool TweetHasImages(ITweet tweet)
        {
            return tweet.Media.Any(m => m.MediaType == "photo");
        }
    }
}

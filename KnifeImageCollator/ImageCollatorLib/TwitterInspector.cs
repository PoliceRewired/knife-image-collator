using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi;

namespace ImageCollatorLib
{
    public class TwitterInspector
    {
        private TwitterClient client;

        public TwitterInspector(string consumerKey, string consumerSecret, Action<string> logger)
        {
            // access tokens not required - this is read only
            this.client = new TwitterClient(consumerKey, consumerSecret);
            this.Log = logger;
        }

        private Action<string> Log;

        public async Task<IEnumerable<MinimalTweet>> ReadTimelineAsync(string username, DateTime earliestDate, DateTime latestDate)
        {
            // compose search criteria
            var criteria = string.Format("from:{0} since:{1} until:{2}",
                username,
                earliestDate.ToString("yyyy-MM-dd"),
                latestDate.ToString("yyyy-MM-dd"));

            Log(string.Format("Search criteria: {0}", criteria));

            // search
            var notedTweets = new List<MinimalTweet>();
            var searchIterator = client.Search.GetSearchTweetsIterator(criteria);
            while (!searchIterator.Completed)
            {
                var searchPage = await searchIterator.NextPageAsync();
                foreach (var tweet in searchPage)
                {
                    // if the tweet has images, extract for analysis
                    if (tweet.Media.Any(m => m.MediaType == "photo"))
                    {
                        var minimalTweet = new MinimalTweet()
                        {
                            Created = tweet.CreatedAt.DateTime,
                            Text = tweet.Text,
                            Username = tweet.CreatedBy.ScreenName,
                            UserId = tweet.CreatedBy.Id,
                            ImageUrls = tweet.Media.Select(m => m.MediaURLHttps)
                        };
                        notedTweets.Add(minimalTweet);
                    }                    
                }
            }

            return notedTweets;
        }

    }
}

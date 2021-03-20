using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib.Inspectors
{
    public class TwitterInspector
    {
        protected TwitterClient client;
        protected Func<ITweet, bool> tweetFilter;
        protected Func<IMediaEntity, bool> mediaFilter;
        protected readonly Filters filter;
        protected Action<string> log;

        public TwitterInspector(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Filters filter, Action<string> logger)
        {
            this.log = logger;
            this.client = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            this.filter = filter;
            this.tweetFilter = FilterHelper.ParseTweetFilter(filter);
            this.mediaFilter = FilterHelper.ParseMediaFilter(filter);
        }

        protected void Log(string message)
        {
            log(message);
        }

        public async Task<IEnumerable<MediaDetails>> FilterTimelineAsync(string username, DateTime earliestDate, DateTime latestDate)
        {
            // compose search criteria
            var criteria = string.Format("from:{0} since:{1} until:{2}",
                username,
                earliestDate.ToString("yyyy-MM-dd"),
                latestDate.ToString("yyyy-MM-dd"));

            Log(string.Format("Search criteria: {0}", criteria));

            // search
            var collation = new List<MediaDetails>();
            var searchIterator = client.Search.GetSearchTweetsIterator(criteria);
            while (!searchIterator.Completed)
            {
                var searchPage = await searchIterator.NextPageAsync();
                foreach (var tweet in searchPage.Where(tweetFilter))
                {
                    var medias = tweet.Media.Where(mediaFilter);
                    var details = medias.Select((m,i) => MediaDetails.From(tweet, m, i));
                    collation.AddRange(details);
                }
            }

            return collation;
        }

        public static bool TweetHasImages(ITweet tweet)
        {
            return tweet.Media.Any(m => m.MediaType == "photo");
        }
    }
}

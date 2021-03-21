using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;
using Tweetinvi.Parameters;

namespace ImageCollatorLib.Inspectors
{
    public class TwitterInspector
    {
        private static readonly int API_LIMIT_MAX_TWEETS = 3200;

        protected TwitterClient client;
        protected IEnumerable<string> keywords;
        protected Func<ITweet, bool> tweetFilter;
        protected Func<IMediaEntity, bool> mediaFilter;
        protected readonly Filters filter;
        protected Action<string> log;

        public TwitterInspector(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, Filters filter, Action<string> logger, IEnumerable<string> keywords)
        {
            this.log = logger;
            this.client = new TwitterClient(consumerKey, consumerSecret, accessToken, accessTokenSecret);
            client.Config.RateLimitTrackerMode = RateLimitTrackerMode.TrackAndAwait;

            this.filter = filter;
            this.keywords = keywords;
            this.tweetFilter = FilterHelper.ParseTweetFilter(filter, keywords);
            this.mediaFilter = FilterHelper.ParseMediaFilter(filter, keywords);
        }

        public async Task<IEnumerable<MediaDetails>> FilterTimelineAsync(string username, DateTime earliest, DateTime latest)
        {
            Log("Examining account timeline: " + username);

            var tweets = await RetrieveTweets(username, earliest, latest);
            var filtered = tweets.Where(tweetFilter);
            var medias = filtered.SelectMany(t => t.Media.Where(mediaFilter).Select((m, i) => MediaDetails.From(t, m, i)));

            Log("Total tweets found:    " + tweets.Count());
            Log("Total tweets included: " + filtered.Count());
            Log("Total media items:     " + medias.Count());

            return medias;
        }

        private async Task<IEnumerable<ITweet>> RetrieveTweets(string username, DateTime earliest, DateTime latest)
        {
            var tweets = new List<ITweet>();
            IEnumerable<ITweet> fetched;

            var timelineParams = new GetUserTimelineParameters(username)
            {
                IncludeEntities = true,
                IncludeRetweets = true,
                PageSize = 200
            };

            var finish = false;
            do
            {
                fetched = await client.Timelines.GetUserTimelineAsync(timelineParams);
                Log("Fetched page with: " + fetched.Count() + " tweets.");

                var fetchedInRange = fetched.Where(t => t.CreatedAt >= earliest && t.CreatedAt < latest);
                Log("➡ Found " + fetchedInRange.Count() + " tweets in range.");
                tweets.AddRange(fetchedInRange);

                if (fetched.Count() > 0)
                {
                    timelineParams.MaxId = fetched.Min(t => t.Id) - 1; Log("➡ New max id: " + timelineParams.MaxId);
                }

                finish = fetched.Count() == 0 || fetched.All(t => t.CreatedAt < earliest) || tweets.Count() >= API_LIMIT_MAX_TWEETS;
            } while (!finish);

            return tweets;
        }

        protected void Log(string message)
        {
            log(message);
        }

    }
}

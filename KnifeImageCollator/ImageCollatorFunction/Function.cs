using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3.Model;
using ImageCollatorLib;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace ImageCollatorFunction
{
    public class Function
    {
        ILambdaContext context;

        public async Task<string> FunctionHandler(string input, ILambdaContext context)
        {
            this.context = context;
            var twitterApiKey = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY");
            var twitterApiKeySecret = Environment.GetEnvironmentVariable("TWITTER_CONSUMER_KEY_SECRET");
            var twitterAccessToken = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN");
            var twitterAccessTokenSecret = Environment.GetEnvironmentVariable("TWITTER_ACCESS_TOKEN_SECRET");
            var bucket = Environment.GetEnvironmentVariable("AWS_S3_BUCKET");

            var filter = ImageCollatorLib.Filter.imagesonly;
            var tweetFilter = FilterHelper.ParseTweetFilter(filter);
            var mediaFilter = FilterHelper.ParseMediaFilter(filter);
            var dates = PeriodHelper.ParsePeriod("today");
            var start = dates[0];
            var end = dates[1];

            Log(input);
            var accounts = input.Split(",");

            var inspector = new TwitterInspector(
                twitterApiKey, twitterApiKeySecret,
                twitterAccessToken, twitterAccessTokenSecret,
                context.Logger.LogLine);

            var collator = new TweetCollator(
                filter, CollateAction.s3,
                "s3-test-group",
                "tweets.csv",
                Log,
                bucket);

            var tweets = await inspector.FilterTimelineAsync(
                "instantiator",
                start, end,
                tweetFilter,
                mediaFilter);

            await collator.CollateAsync(tweets);

            return string.Format("Retrieved images from {0} tweets.", tweets.Count());
        }

        public void Log(string message)
        {
            context.Logger.LogLine(message);
        }
    }
}

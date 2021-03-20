using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon;
using Amazon.Lambda.Core;
using Amazon.S3.Model;
using ImageCollatorLib;
using ImageCollatorLib.Collation;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using ImageCollatorLib.Inspectors;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace ImageCollatorFunction
{
    public class Function
    {
        ILambdaContext context;

        public async Task<ImageCollatorOutputs> FunctionHandler(ImageCollatorInputs input, ILambdaContext context)
        {
            this.context = context;

            Log("Collation: " + input.collation);
            Log("Accounts:  " + input.accounts);
            Log("Period:    " + input.period);
            Log("Group:     " + input.group);

            var collation = EnumHelper.EnsureArgument<Collations>(input.collation, "Action");

            var twitterApiKey = GetEnv("TWITTER_CONSUMER_KEY");
            var twitterApiKeySecret = GetEnv("TWITTER_CONSUMER_KEY_SECRET");
            var twitterAccessToken = GetEnv("TWITTER_ACCESS_TOKEN");
            var twitterAccessTokenSecret = GetEnv("TWITTER_ACCESS_TOKEN_SECRET");
            var githubToken = GetEnv("GITHUB_TOKEN", collation == Collations.github);
            var githubRepository = GetEnv("GITHUB_REPOSITORY", collation == Collations.github);
            var bucket = GetEnv("AWS_S3_BUCKET", collation == Collations.s3);

            var filter = Filters.images;
            var tweetFilter = FilterHelper.ParseTweetFilter(filter);
            var mediaFilter = FilterHelper.ParseMediaFilter(filter);

            var dates = PeriodHelper.ParsePeriod(input.period);
            var start = dates[0];
            var end = dates[1];

            var accounts = input.accounts.Split(",");

            var inspector = new TwitterInspector(
                twitterApiKey,
                twitterApiKeySecret,
                twitterAccessToken,
                twitterAccessTokenSecret,
                filter,
                context.Logger.LogLine);

            ICollator collator = CollatorFactory.Create(collation, Log, input.group, bucket);

            var outputs = new ImageCollatorOutputs();

            foreach (var account in accounts)
            {
                var medias = await inspector.FilterTimelineAsync(account, start, end);
                var summary = await collator.CollateAsync(medias);

                outputs.summaries += summary.Summaries;
                outputs.files += summary.Files;
                outputs.errors.AddRange(summary.Errors);
            }

            return outputs;
        }

        public string GetEnv(string key, bool required = true)
        {
            var result = Environment.GetEnvironmentVariable(key);
            if (required && string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentNullException(key, "Environment variable missing.");
            }
            return result;
        }

        public void Log(string message)
        {
            context.Logger.LogLine(message);
        }
    }
}

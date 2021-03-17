using System;
using System.IO;
using System.Threading.Tasks;
using ImageCollatorLib;

namespace KnifeImageCollatorApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var environment = GetArg(args, 0, "environment").Trim().ToLower();
            var username = GetArg(args, 1, "username").Trim().ToLower();
            var periodStr = GetArg(args, 2, "period").Trim().ToLower();
            var filterStr = GetArg(args, 3, "filter").Trim().ToLower();
            var actionStr = GetArg(args, 4, "action").Trim().ToLower();
            var group = GetArg(args, 5, "group").Trim().ToLower();

            var dates = PeriodHelper.ParsePeriod(periodStr);
            var start = dates[0];
            var end = dates[1];

            var tweetFilter = FilterHelper.ParseTweetFilter(filterStr);
            var mediaFilter = FilterHelper.ParseMediaFilter(filterStr);

            Console.WriteLine("Environment: " + environment);
            Console.WriteLine("Username:    " + username);
            Console.WriteLine("Period:      " + periodStr);
            Console.WriteLine("↳ Start:     " + start.ToShortDateString());
            Console.WriteLine("↳ End:       " + end.ToShortDateString());

            var envFile = ".env." + environment;
            DotNetEnv.Env.Load(envFile);
            var twitterApiKey = GetEnv("TWITTER_CONSUMER_KEY");
            var twitterApiKeySecret = GetEnv("TWITTER_CONSUMER_KEY_SECRET");
            var twitterAccessToken = GetEnv("TWITTER_ACCESS_TOKEN");
            var twitterAccessTokenSecret = GetEnv("TWITTER_ACCESS_TOKEN_SECRET");

            var inspector = new TwitterInspector(twitterApiKey, twitterApiKeySecret, twitterAccessToken, twitterAccessTokenSecret, Console.WriteLine);
            var found = await inspector.FilterTimelineAsync(
                username,
                start, end,
                tweetFilter,mediaFilter);

            var collator = TweetCollator.FromArgs(filterStr, actionStr, group, "tweets.csv", Console.WriteLine);
            await collator.CollateAsync(found);
        }

        public static string GetArg(string[] args, int index, string name, bool required = true)
        {
            if (args.Length < index + 1)
            {
                if (required)
                {
                    throw new ArgumentNullException(name, "Argument " + index + " missing. Should contain a value for: " + name);
                }
                else
                {
                    return null;
                }
            }

            var result = args[index];

            if (required && string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentNullException(name, "Argument " + index + " is empty. Should contain a value for: " + name);
            }

            return result;
        }

        public static string GetEnv(string key, bool required = true)
        {
            var result = DotNetEnv.Env.GetString(key);
            Console.WriteLine("Environment variable: " + key + (string.IsNullOrWhiteSpace(result) ? " not found" : " found"));
            if (required && string.IsNullOrWhiteSpace(result))
            {
                throw new ArgumentNullException(key, "The " + key + " environment variable is not set.");
            }
            else
            {
                return result;
            }
        }

    }
}

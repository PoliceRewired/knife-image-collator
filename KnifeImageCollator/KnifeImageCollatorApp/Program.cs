using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ImageCollatorLib;
using ImageCollatorLib.Collation;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using ImageCollatorLib.Inspectors;

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
            var collationStr = GetArg(args, 4, "collation").Trim().ToLower();
            var group = GetArg(args, 5, "group").Trim().ToLower();

            Console.WriteLine("Environment: " + environment);
            Console.WriteLine("Username:    " + username);
            Console.WriteLine("Period:      " + periodStr);
            Console.WriteLine("Filter:      " + filterStr);

            var dates = PeriodHelper.ParsePeriod(periodStr);
            var start = dates[0];
            var end = dates[1];

            Console.WriteLine("↳ Start:     " + start.ToShortDateString());
            Console.WriteLine("↳ End:       " + end.ToShortDateString());

            var filter = EnumHelper.EnsureArgument<Filters>(filterStr, "filter");
            var collation = EnumHelper.EnsureArgument<Collations>(collationStr, "collation");

            var envFile = ".env." + environment;
            DotNetEnv.Env.Load(envFile);
            var twitterApiKey = GetEnv("TWITTER_CONSUMER_KEY");
            var twitterApiKeySecret = GetEnv("TWITTER_CONSUMER_KEY_SECRET");
            var twitterAccessToken = GetEnv("TWITTER_ACCESS_TOKEN");
            var twitterAccessTokenSecret = GetEnv("TWITTER_ACCESS_TOKEN_SECRET");
            var bucket = GetEnv("AWS_S3_BUCKET", collation == Collations.s3);
            var githubToken = GetEnv("GITHUB_TOKEN", collation == Collations.github);
            var githubOwner = GetEnv("GITHUB_OWNER", collation == Collations.github);
            var githubRepository = GetEnv("GITHUB_REPOSITORY", collation == Collations.github);

            var inspector = new TwitterInspector(
                twitterApiKey,
                twitterApiKeySecret,
                twitterAccessToken,
                twitterAccessTokenSecret,
                filter,
                Console.WriteLine);

            var found = await inspector.FilterTimelineAsync(username, start, end);
            var collator = CollatorFactory.Create(
                collation,
                Console.WriteLine,
                group,
                s3bucket: bucket,
                githubToken: githubToken,
                githubOwner: githubOwner,
                githubRepository: githubRepository);

            collator.Verbose = true;

            var result = await collator.CollateAsync(found);

            Console.WriteLine(string.Format("Summaries: {0}", result.Summaries));
            Console.WriteLine(string.Format("Files:     {0}", result.Files));
            Console.WriteLine(string.Format("Errors:\n{0}", string.Join('\n',result.Errors)));
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

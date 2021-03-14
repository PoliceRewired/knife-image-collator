using System;
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

            var envFile = ".env." + environment;
            DotNetEnv.Env.Load(envFile);
            var twitterApiKey = GetEnv("TWITTER_CONSUMER_KEY");
            var twitterApiKeySecret = GetEnv("TWITTER_CONSUMER_KEY_SECRET");
            var twitterAccessToken = GetEnv("TWITTER_ACCESS_TOKEN");
            var twitterAccessTokenSecret = GetEnv("TWITTER_ACCESS_TOKEN_SECRET");

            Console.WriteLine("Reading today's tweets for: " + username);
            var inspector = new TwitterInspector(twitterApiKey, twitterApiKeySecret, twitterAccessToken, twitterAccessTokenSecret, Console.WriteLine);
            var found = await inspector.FilterTimelineAsync(
                username,
                DateTime.Now.Date,
                DateTime.Now.AddDays(1).Date,
                (tweet) => true);

            foreach (var mt in found)
            {
                Console.WriteLine(mt.Text);
            }
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
            Console.WriteLine("Environment variable: " + key + (string.IsNullOrWhiteSpace(result) ? " found" : " not found"));
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

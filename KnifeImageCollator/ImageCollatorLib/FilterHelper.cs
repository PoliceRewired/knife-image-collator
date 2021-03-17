using System;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib
{
    public class FilterHelper
    {
        public static E EnsureArgument<E>(string arg, string argName) where E:struct
        {
            E argument;
            var ok = Enum.TryParse(arg, out argument);
            if (ok)
            {
                return argument;
            }
            else
            {
                throw new ArgumentException(string.Format("Urecognised: {0}", arg), argName);
            }
        }

        public static Func<ITweet, bool> ParseTweetFilter(string filter)
        {
            return ParseTweetFilter(EnsureArgument<Filter>(filter, "filter"));
        }

        public static Func<ITweet, bool> ParseTweetFilter(Filter filter)
        {
            switch (filter)
            {
                case Filter.all:
                    return (tweet) => true;

                case Filter.imagesonly:
                    return (tweet) => tweet.Media != null && tweet.Media.Count > 0;

                default:
                    throw new NotImplementedException("Filter not implemented: " + filter.ToString());
            }
        }

        public static Func<IMediaEntity, bool> ParseMediaFilter(string filter)
        {
            return ParseMediaFilter(EnsureArgument<Filter>(filter, "filter"));
        }

        public static Func<IMediaEntity, bool> ParseMediaFilter(Filter filter)
        {
            switch (filter)
            {
                case Filter.all:
                    return (tweet) => true;

                case Filter.imagesonly:
                    return (media) => media.MediaType == "photo";

                default:
                    throw new NotImplementedException("Filter not implemented: " + filter.ToString());
            }
        }

    }
}

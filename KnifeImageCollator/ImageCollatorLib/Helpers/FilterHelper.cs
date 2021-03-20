using System;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib.Helpers
{
    public class FilterHelper
    {
        public static Func<ITweet, bool> ParseTweetFilter(string filter)
        {
            return ParseTweetFilter(EnumHelper.EnsureArgument<Filters>(filter, "filter"));
        }

        public static Func<ITweet, bool> ParseTweetFilter(Filters filter)
        {
            switch (filter)
            {
                case Filters.images:
                    return (tweet) => tweet.Media != null && tweet.Media.Count > 0;

                default:
                    throw new NotImplementedException("Filter not implemented: " + filter.ToString());
            }
        }

        public static Func<IMediaEntity, bool> ParseMediaFilter(string filter)
        {
            return ParseMediaFilter(EnumHelper.EnsureArgument<Filters>(filter, "filter"));
        }

        public static Func<IMediaEntity, bool> ParseMediaFilter(Filters filter)
        {
            switch (filter)
            {
                case Filters.images:
                    return (media) => media.MediaType == "photo";

                default:
                    throw new NotImplementedException("Filter not implemented: " + filter.ToString());
            }
        }

    }
}

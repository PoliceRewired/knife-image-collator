using System;
using System.Collections.Generic;
using System.Linq;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using Tweetinvi.Models;
using Tweetinvi.Models.Entities;

namespace ImageCollatorLib.Helpers
{
    public class FilterHelper
    {
        public static Func<ITweet, bool> ParseTweetFilter(string filter, IEnumerable<string> keywords)
        {
            return ParseTweetFilter(EnumHelper.EnsureArgument<Filters>(filter, "filter"), keywords);
        }

        public static Func<ITweet, bool> ParseTweetFilter(Filters filter, IEnumerable<string> keywords)
        {
            switch (filter)
            {
                case Filters.images:
                    return (tweet) =>
                        (tweet.ExtendedTweet != null &&
                        tweet.ExtendedTweet.ExtendedEntities.Medias != null &&
                        tweet.ExtendedTweet.ExtendedEntities.Medias.Count > 0) ||
                        (tweet.Entities.Medias != null &&
                        tweet.Entities.Medias.Count > 0);

                case Filters.keywords:
                    return (tweet) =>
                        (tweet.ExtendedTweet != null &&
                        tweet.ExtendedTweet.ExtendedEntities.Medias != null &&
                        tweet.ExtendedTweet.ExtendedEntities.Medias.Count > 0) ||
                        (tweet.Entities.Medias != null &&
                        tweet.Entities.Medias.Count > 0) &&
                        keywords.Any(k => tweet.Text.ToLower().Contains(k));

                default:
                    throw new NotImplementedException("Filter not implemented: " + filter.ToString());
            }
        }

        public static Func<IMediaEntity, bool> ParseMediaFilter(string filter, IEnumerable<string> keywords)
        {
            return ParseMediaFilter(EnumHelper.EnsureArgument<Filters>(filter, "filter"), keywords);
        }

        public static Func<IMediaEntity, bool> ParseMediaFilter(Filters filter, IEnumerable<string> keywords)
        {
            // for now, all known filters return photos only
            return (media) => media.MediaType == "photo";
        }

    }
}

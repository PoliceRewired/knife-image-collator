using System;
using System.Collections.Generic;
using System.Linq;
using CsvHelper.Configuration.Attributes;

namespace ImageCollatorLib
{
    public class MinimalTweetDTO
    {
        [Ignore]
        public DateTime Created { get; set; }

        public string CreatedUTC
        {
            get
            {
                return Created.ToUniversalTime().ToString("O");
            }
        }

        public long CreatedUnixTimeMs
        {
            get
            {
                return ((DateTimeOffset)Created).ToUnixTimeMilliseconds();
            }
        }

        public long TweetId { get; set; }
        public long UserId { get; set; }
        public string Username { get; set; }
        public string Text { get; set; }
        public int Attachments => ImageUrls != null ? ImageUrls.Count() : 0;

        [Ignore]
        public IEnumerable<string> ImageUrls { get; set; }

    }
}

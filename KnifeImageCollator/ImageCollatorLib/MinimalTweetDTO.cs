using System;
using System.Collections.Generic;

namespace ImageCollatorLib
{
    public class MinimalTweetDTO
    {
        public DateTime Created { get; set; }
        public string Username { get; set; }
        public long UserId { get; set; }
        public long TweetId { get; set; }
        public string Text { get; set; }
        public IEnumerable<string> ImageUrls { get; set; }

    }
}

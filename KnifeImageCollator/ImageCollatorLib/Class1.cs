using System;
using Tweetinvi;

namespace ImageCollatorLib
{
    public class TwitterInspector
    {
        private TwitterClient client;

        public TwitterInspector(string consumerKey, string consumerSecret, string accessToken, string accessSecret, Action<string> logger)
        {
            this.client = new TwitterClient(consumerKey, consumerSecret, accessToken, accessSecret);
            this.Log = logger;
        }

        private Action<string> Log;

        public void ReadTimeline(string username, DateTime earliest)
        {
            Log("Reading timeline for: " + username + ", from: " + earliest.ToShortDateString() + " " + earliest.ToShortTimeString());


        }

    }
}

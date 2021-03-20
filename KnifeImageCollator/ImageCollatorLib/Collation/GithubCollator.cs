using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;
using Octokit;

namespace ImageCollatorLib.Collation
{
    public class GithubCollator : AbstractCollator
    {
        string repo;
        Credentials credentials;
        GitHubClient client;

        public GithubCollator(string repo, string token, string group, Action<string> log) : base(group, log)
        {
            this.repo = repo;
            this.credentials = new Credentials(token);
        }

        protected override async Task InitTransactionAsync()
        {
            client = client ?? new GitHubClient(new ProductHeaderValue(GetType().FullName));
        }

        protected override async Task CommitTransactionAsync()
        {
        }

        protected override async Task AppendCsvAsync(IEnumerable<MediaDetails> medias, string path)
        {



        }

        protected override async Task TransferImageAsync(string url, string path)
        {
            throw new NotImplementedException();
        }
    }
}

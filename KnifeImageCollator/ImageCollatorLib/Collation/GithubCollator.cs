using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;
using ImageCollatorLib.Helpers;
using Octokit;

namespace ImageCollatorLib.Collation
{
    /// <summary>
    /// See: https://laedit.net/2016/11/12/GitHub-commit-with-Octokit-net.html
    /// </summary>
    public class GithubCollator : AbstractCollator
    {
        string repo;
        string owner;
        Credentials credentials;
        GitHubClient github;
        string mainBranch = "main";
        Commit lastCommit;
        Reference masterReference;
        NewTree newTree;
        string headMasterRef;

        public GithubCollator(string owner, string repo, string token, string group, Action<string> log) : base(group, log)
        {
            this.owner = owner;
            this.repo = repo;
            this.credentials = new Credentials(token);
        }

        protected override async Task InitTransactionAsync()
        {
            github = github ?? new GitHubClient(new ProductHeaderValue(GetType().FullName));
            github.Credentials = credentials;

            headMasterRef = "heads/" + mainBranch;
            masterReference = await github.Git.Reference.Get(owner, repo, headMasterRef);
            lastCommit = await github.Git.Commit.Get(owner, repo, masterReference.Object.Sha);
            newTree = new NewTree { BaseTree = lastCommit.Tree.Sha };
        }

        protected override async Task CommitTransactionAsync()
        {
            var newTreeResult = await github.Git.Tree.Create(owner, repo, newTree);
            var newCommit = new NewCommit("Commit test with several files", newTreeResult.Sha, masterReference.Object.Sha);
            var commit = await github.Git.Commit.Create(owner, repo, newCommit);
            await github.Git.Reference.Update(owner, repo, headMasterRef, new ReferenceUpdate(commit.Sha));
        }

        protected override async Task<IEnumerable<MediaDetails>> ReadCurrentCsvAsync(string path)
        {
            try
            {
                var csvContents = await github.Repository.Content.GetAllContentsByRef(owner, repo, path, masterReference.Object.Sha);
                var exists = csvContents.Count() == 1;
                if (exists)
                {
                    Log(string.Format("Found: {0}", path));
                    Log("Reading CSV...");
                    var csvText = csvContents.First().Content;
                    //TODO: you are here
                    Log("CSV:\n" + csvText);
                    var csvTextNoBlankLines = string.Join('\n', csvText.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)));
                    return CsvFileHelper.ReadCsvText(csvTextNoBlankLines);
                }
                else
                {
                    return new List<MediaDetails>();
                }
            }
            catch (NotFoundException)
            {
                Log(string.Format("Not found: {0}", path));
                Log("Preparing new file...");
                return new List<MediaDetails>();
            }
        }

        protected override async Task StoreNewCsvAsync(IEnumerable<MediaDetails> medias, string path)
        {
            var newCsvText = await CsvFileHelper.CsvTextFromMediaAsync(medias);
            var newTreeItem = new NewTreeItem { Mode = "100644", Type = TreeType.Blob, Content = newCsvText, Path = path };
            newTree.Tree.Add(newTreeItem);
        }

        protected override async Task TransferImageAsync(string url, string path)
        {
            var uri = new Uri(url);
            var data = new MemoryStream();
            using (var client = new WebClient())
            {
                using (var downloadStream = await client.OpenReadTaskAsync(uri))
                {
                    await downloadStream.CopyToAsync(data);
                    data.Position = 0;
                    var imgBase64 = Convert.ToBase64String(data.ToArray());

                    var imgBlob = new NewBlob { Encoding = EncodingType.Base64, Content = imgBase64 };
                    var imgBlobRef = await github.Git.Blob.Create(owner, repo, imgBlob);
                    newTree.Tree.Add(new NewTreeItem { Path = path, Mode = "100644", Type = TreeType.Blob, Sha = imgBlobRef.Sha });
                }
            }

        }
    }
}

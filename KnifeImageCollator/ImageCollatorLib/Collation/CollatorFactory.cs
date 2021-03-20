using System;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Collation
{
    public class CollatorFactory
    {
        public static ICollator Create(Collations collation, Action<string> log, string group, string s3bucket = null, string githubToken = null, string githubOwner = null, string githubRepository = null)
        {
            switch (collation)
            {
                case Collations.download:
                    return new DownloadCollator(group, log);
                    
                case Collations.s3:
                    return new S3Collator(group, s3bucket, log);
                    
                case Collations.github:
                    return new GithubCollator(githubOwner, githubRepository, githubToken, group, log);

                case Collations.list:
                    return new ListCollator(group, log);

                default:
                    throw new NotImplementedException(collation.ToString());
            }
        }
    }
}

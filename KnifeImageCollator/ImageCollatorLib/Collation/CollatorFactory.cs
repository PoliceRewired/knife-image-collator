using System;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Collation
{
    public class CollatorFactory
    {
        public static ICollator Create(Collations collation, Action<string> log, string group, string bucket)
        {
            switch (collation)
            {
                case Collations.download:
                    return new DownloadCollator(group, log);
                    
                case Collations.s3:
                    return new S3Collator(group, bucket, log);
                    
                case Collations.github:
                    throw new NotImplementedException();

                case Collations.list:
                    return new ListCollator(group, log);

                default:
                    throw new NotImplementedException(collation.ToString());
            }
        }
    }
}

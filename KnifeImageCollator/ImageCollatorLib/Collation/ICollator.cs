using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Collation
{
    public interface ICollator
    {
        Task<CollationSummary> CollateAsync(IEnumerable<MediaDetails> tweets);

    }
}

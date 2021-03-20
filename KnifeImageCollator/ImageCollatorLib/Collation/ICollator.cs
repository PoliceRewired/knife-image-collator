using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ImageCollatorLib.Entities;

namespace ImageCollatorLib.Collation
{
    public interface ICollator
    {
        bool Verbose { get; set; }
        Task<CollationSummary> CollateAsync(IEnumerable<MediaDetails> tweets);

    }
}

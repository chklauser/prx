using System;
using System.Collections.Generic;
using System.Text;

namespace Prx.Benchmarking
{
    public sealed  class BenchmarkEntryCollection : System.Collections.ObjectModel.KeyedCollection<string, BenchmarkEntry>
    {

        public  BenchmarkEntryCollection()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        ///<summary>
        ///When implemented in a derived class, extracts the key from the specified element.
        ///</summary>
        ///
        ///<returns>
        ///The key for the specified element.
        ///</returns>
        ///
        ///<param name="item">The element from which to extract the key.</param>
        protected override string GetKeyForItem(BenchmarkEntry item)
        {
            return item.Function.Id;
        }
    }
}

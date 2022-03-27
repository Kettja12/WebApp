using Services.Models;
using System.Collections.Concurrent;

namespace Services

{
    public partial class SessionServices
    {
        public object lockObj = new object();
        public readonly ConcurrentDictionary<int,Transaction> transactions =
            new ConcurrentDictionary<int,Transaction>();
        private readonly ConcurrentDictionary<int, TRItem> insertedList =
            new ConcurrentDictionary<int, TRItem>();
        private readonly ConcurrentDictionary<int, TRItem> modifiedList =
            new ConcurrentDictionary<int, TRItem>();
        private readonly ConcurrentDictionary<int, TRItem> deletedList =
            new ConcurrentDictionary<int, TRItem>();
    }
}
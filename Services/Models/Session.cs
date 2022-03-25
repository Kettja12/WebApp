using Services.Models;
using System.Collections.Concurrent;

namespace Services

{
    public partial class SessionServices
    {
        public object lockObj = new object();
        public readonly ConcurrentDictionary<int,Transaction> transactions =
            new ConcurrentDictionary<int,Transaction>();
        private readonly ConcurrentDictionary<int,TRUser> userList = 
            new ConcurrentDictionary<int,TRUser>();
    }
}
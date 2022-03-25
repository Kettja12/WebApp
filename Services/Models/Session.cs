using DBContext;
using Services.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Services

{
    public partial class SessionServices
    {
        public Object lockObj = new Object();
        public readonly ConcurrentDictionary<int,Transaction> transactions =
            new ConcurrentDictionary<int,Transaction>();
        private readonly ConcurrentDictionary<int,TRUser> userList = 
            new ConcurrentDictionary<int,TRUser>();
    }
}
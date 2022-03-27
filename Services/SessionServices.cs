using DBContext;
using Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public partial class SessionServices
    {
        protected string connectionString { get; private set; }
        private int counter;
        private int addCounter;

        private static SessionServices instance = null;
        public static SessionServices GetInstance
        {
            get
            {
                if (instance == null)
                    instance = new SessionServices();
                return instance;
            }
        }

        private SessionServices()
        {
        }
        public void SetConnectionString(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public void PrintDetails(string message)
        {
            Console.WriteLine(message);
        }

        public int GetId()
        {
            int result = 0;
            lock (lockObj)
            {
                counter++;
                result = counter;
            }
            return result;
        }

        public int GetAddId()
        {
            int result = 0;
            lock (lockObj)
            {
                addCounter--;
                result = addCounter;
            }
            return result;
        }

        public SessionServices(string connectionString)
        {
            this.connectionString = connectionString;
        }
        public Transaction StartTransaction(string sessionID, string username)
        {
            var newId = GetId();
            var transaction = new Transaction(newId, sessionID, username);
            transactions.TryAdd(newId, transaction);
            return transaction;

        }
        public bool RollbackTransaction(Transaction transaction)
        {
            if (transactions.TryRemove(transaction.Id, out transaction))
            {
                return true;
            }
            return false;
        }

        public Transaction GetTransaction(int id)
        {
            return transactions.FirstOrDefault(x => x.Key == id).Value;
        }

        public int GetSessionTransactionCount(string session)
        {
            return transactions.Where(x => x.Value.SessionID == session).Count();
        }

        public bool UndoTransaction(string session)
        {
            var tr = transactions.Where(x => x.Value.SessionID == session)
                .OrderByDescending(x => x.Key).FirstOrDefault();
            if (RemoveTransaction(tr.Value))
            {
                if (transactions.TryRemove(tr.Key, out Transaction trvalue))
                    return true;
            };

            return false;
        }

        public bool UndoAllTransaction(string session)
        {
            var trs = transactions.Where(x => x.Value.SessionID == session)
                .OrderByDescending(x => x.Key);
            foreach (var tr in trs)
            {
                if (RemoveTransaction(tr.Value))
                {
                    if (transactions.TryRemove(tr.Key, out Transaction trvalue)==false)
                        return false;
                };
            }

            return true;
        }

        public async Task<bool> SavetoDbAllTransactionsAsync(string session)
        {
            var newIds = new Dictionary<int, int>();
            var trs = transactions
                .Where(x => x.Value.SessionID == session)
                .OrderBy(x => x.Key).ToList();
            foreach (var tr in trs)
            {
                newIds = await SaveToDbTransaction(tr.Value, newIds);
                if (newIds==null) return false;
            }
            UndoAllTransaction(session);
            return true;
        }

        public async Task<Dictionary<int, int>> SaveToDbTransaction(Transaction transaction,
            Dictionary<int, int> newIds)
        {
            using (HelloContext context = new HelloContext(connectionString))
            {
                using (var tr = await context.Database.BeginTransactionAsync())
                {
                        newIds = await SaveUsersToDb(context, transaction,newIds);
                        if (newIds!=null)
                            context.Database.CommitTransaction();
                        else
                            context.Database.RollbackTransaction();
                        return newIds;
                }
            }
        }
        public bool RemoveTransaction(Transaction transaction)
        {
            return RemoveInsetedTransaction(transaction)
                && RemoveModifiedTransaction(transaction)
                && RemoveDeletedTransaction(transaction);
        }
        private bool RemoveInsetedTransaction(Transaction transaction)
        {
            IEnumerable<KeyValuePair<int, TRItem>> items = insertedList
                .Where(x => x.Value.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (insertedList.TryRemove(item.Key, out TRItem trUser) == false)
                {
                    return false;
                }
            }
            return true;
        }
        private bool RemoveModifiedTransaction(Transaction transaction)
        {
            IEnumerable<KeyValuePair<int, TRItem>> items = modifiedList
                .Where(x => x.Value.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (modifiedList.TryRemove(item.Key, out TRItem trUser) == false)
                {
                    return false;
                }
            }
            return true;
        }
        private bool RemoveDeletedTransaction(Transaction transaction)
        {
            IEnumerable<KeyValuePair<int, TRItem>> items = deletedList
                .Where(x => x.Value.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (deletedList.TryRemove(item.Key, out TRItem trUser) == false)
                {
                    return false;
                }
            }
            return true;
        }


    }
}

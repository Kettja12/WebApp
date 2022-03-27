using DBContext;
using Microsoft.EntityFrameworkCore;
using Services.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Services
{
    public partial class SessionServices
    {

        private bool IsDeletedUser(int id)
        {
            return deletedList.Values.LastOrDefault(x => x.User!=null&& x.User.Id == id)!=null;
        }

        public TRItem IsUserModified(int id)
        {
            var isModified = modifiedList.Values.LastOrDefault(x => x.User != null && x.User.Id == id);
            if (isModified == null)
            {
                isModified = insertedList.Values.LastOrDefault(x => x.User != null && x.User.Id == id);
            }

            return isModified;
        }
        public List<User> GetModifiedUsers()
        {
            List<User> modified = modifiedList.Values
                .Where(x => x.User != null)
                .Select(x => x.User)
                .ToList();
            return modified;
        }

        public List<User> GetInsertedUsers()
        {
            List<User> inserted = insertedList.Values
                .Where (x => x.User!=null)
                .Select(x => x.User)
                .ToList();
            return inserted;
        }
        public string SetUser(Transaction transaction, User user)
        {
            var modifiedUser = IsUserModified(user.Id);
            if (modifiedUser != null)
            {
                if (modifiedUser.Transaction.SessionID != transaction.SessionID)
                {
                    return "Tietue toisen istunnon muokkaama ja tallentamatta ei voi muokata: "
                        + modifiedUser.Transaction.Username;
                }

            }
            var trUser = new TRItem(transaction, user);
            modifiedList.TryAdd(GetId(), trUser);
            return "";
        }
        public string DeleteUser(Transaction transaction, User user)
        {
            var modifiedUser = IsUserModified(user.Id);
            if (modifiedUser != null)
            {
                if (modifiedUser.Transaction.SessionID != transaction.SessionID)
                {
                    return "Tietue toisen istunnon muokkaama ja tallentamatta ei voi muokata: "
                        + modifiedUser.Transaction.Username;
                }

            }
            var trUser = new TRItem(transaction, user);
            deletedList.TryAdd(GetId(), trUser);
            return "";
        }
        public string AddUser(Transaction transaction, User user)
        {
            user.Id = GetAddId();
            user.Username = "uusi";
            foreach(var item in user.Claims){
                item.UserId = user.Id;
                item.Id = GetAddId();
            }
            var trUser = new TRItem(transaction, user);
            insertedList.TryAdd(GetId(), trUser);
            return "";
        }
        public async Task<List<User>> GetUsersAsync()
        {
            using (var context = new HelloContext(connectionString))
            {

                List<User> usersFromdb = null;
                var deletedIDs = deletedList.Values
                    .Where(x=>x.User!=null).Select(x=>x.User.Id).ToList();
                if (deletedIDs.Any())
                {
                    usersFromdb = await context.Users
                        .Where(x=>deletedIDs.Contains(x.Id)==false)
                         .Include(c => c.Claims)
                         .ToListAsync();

                }
                else
                {
                    usersFromdb = await context.Users
                         .Include(c => c.Claims)
                         .ToListAsync();

                }
                usersFromdb.AddRange(GetInsertedUsers());
                var users = new List<User>();
                foreach (User user in GetModifiedUsers())
                {
                    var index = usersFromdb.FindIndex(x => x.Id == user.Id);
                    if (index != -1)
                    {
                        usersFromdb[index] = user;
                    }
                }
                return usersFromdb;
            }
        }
        public async Task<Dictionary<int, int>> SaveUsersToDb(
            HelloContext context,
            Transaction transaction,
            Dictionary<int, int> newIds)
        {
            var result = await SaveDeletedUsersToDb(context, transaction);
            if (result)
                newIds = await SaveInsertedUsersToDb(context, transaction, newIds);
            if (newIds!=null)
                newIds = await SaveModifiedUsersToDb(context, transaction, newIds);
            return newIds;
        }

        private async Task<bool> SaveDeletedUsersToDb(
          HelloContext context,
          Transaction transaction)
        {
            var items = deletedList.Values
                       .Where(x => x.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                var result = await context.DeleteUserAsync(
                    item.User);
            }
            return true;
        }

        private async Task<Dictionary<int, int>> SaveInsertedUsersToDb(
          HelloContext context,
          Transaction transaction,
          Dictionary<int, int> newIds)
        {
            var items = insertedList.Values
                       .Where(x => x.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                var insertId = item.User.Id;
                var user = await context.SaveUserAsync(
                    item.User, null);
                newIds.Add(insertId, user.Id);
                item.User = user;
                foreach (var claim in item.User.Claims)
                {
                    insertId=claim.Id;
                    Claim newclaim = await context.SaveClaimAsync(
                        claim);
                    newIds.Add(insertId, claim.Id);
                }
            }
            return newIds;
        }

        private async Task<Dictionary<int, int>> SaveModifiedUsersToDb(
          HelloContext context,
          Transaction transaction,
          Dictionary<int, int> newIds)
        {
            var items = modifiedList.Values
                       .Where(x => x.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (newIds.TryGetValue(item.User.Id,out int newid)){
                    item.User.Id=newid;
                    foreach (var claim in item.User.Claims)
                    {
                        if (newIds.TryGetValue(claim.Id, out newid))
                        {
                            claim.Id = newid;
                        }
                    }
                }

                var userFromdb = await context.Users
                .FirstOrDefaultAsync(
                    x => x.Id == item.User.Id);
                if (userFromdb == null)
                {
                    if (newIds.TryGetValue(item.User.Id, out int id))
                    {
                        item.User.Id = id;
                    }
                }
                item.User = await context.SaveUserAsync(
                item.User, userFromdb);
                foreach (var claim in item.User.Claims)
                {
                    var insertId = claim.Id;
                    Claim newclaim = await context.SaveClaimAsync(
                        claim);
                    if (insertId!=claim.Id)
                        newIds.Add(insertId, claim.Id);

                }

            }
            return newIds;
        }

        public async Task<User> GetUserByUserName(string username)
        {
            using (var context = new HelloContext(connectionString))
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(x => x.Username == username);
                if (user == null) return null;
                if (IsDeletedUser(user.Id))return null;
                var user2 = IsUserModified(user.Id);
                if (user2 != null) return user2.User;
                return user;
            }
        }
    }
}

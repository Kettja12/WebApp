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
        public TRUser IsUserModified(int id)
        {
            return  userList.Values.LastOrDefault(x => x.User.Id == id);
        }
        public List<User> GetInserted()
        {
            List<User> items = userList.Values
                .Where(x => x.Operation == "A")
                .Select(x => x.User)
                .ToList();
            return items;
        }
        public bool RemoveUserTransaction(Transaction transaction)
        {
            IEnumerable<KeyValuePair<int, TRUser>> items = userList
                .Where(x => x.Value.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (userList.TryRemove(item.Key, out TRUser trUser) == false)
                {
                    return false;
                }
            }
            return true;
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
            var trUser = new TRUser(transaction, user, "M");
            userList.TryAdd(GetId(), trUser);
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
            var trUser = new TRUser(transaction, user, "D");
            userList.TryAdd(GetId(), trUser);
            return "";
        }
        public string AddUser(Transaction transaction, User user)
        {
            user.Id = GetAddId();
            var trUser = new TRUser(transaction, user, "A");
            userList.TryAdd(GetId(), trUser);
            return "";
        }
        public async Task<List<User>> getUsersAsync()
        {
            using (var context = new HelloContext(connectionString))
            {
                var usersFromdb = await context.Users
                     .Include(c => c.Claims)
                     .ToListAsync();
                var users = new List<User>();
                foreach (User user in usersFromdb)
                {
                    var user2 = IsUserModified(user.Id);
                    if (user2 != null)
                    {
                        if (user2.Operation != "D")
                            users.Add(user2.User);
                    }
                    else
                    {
                        users.Add(user);
                    }
                }
                users.AddRange(GetInserted());
                return users;
            }
        }
        public async Task<bool> SaveUsersToDb(
            HelloContext context, 
            Transaction transaction,
            Dictionary<int, int> newIds)
        {
            var items = userList.Values
                       .Where(x => x.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (item.Operation == "M")
                {
                    var userFromdb = await context.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == item.User.Id);
                    item.User = await context.SaveUserAsync(
                        item.User, userFromdb);
                }
                if (item.Operation == "A")
                {
                    var user = await context.SaveUserAsync(
                        item.User, null);
                    newIds.Add(item.User.Id, user.Id);
                    item.User = user;
                }
                if (item.Operation == "D")
                {
                    var result = await context.DeleteUserAsync(
                        item.User);
                }
            }
            return true;
        }
        public async Task<User> GetUserByUserName(string username)
        {
            using (var context = new HelloContext(connectionString))
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(x => x.Username == username);
                if (user == null) return null;
                var user2 = IsUserModified(user.Id);
                if (user2!=null) return user2.User;
                return user;
            }
        }
    }
}

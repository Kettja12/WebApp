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
            KeyValuePair<int, TRUser> item = userList.LastOrDefault(x => x.Value.User.Id == id);
            return item.Value;
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
            IEnumerable<KeyValuePair<int, TRUser>> items = userList
                       .Where(x => x.Value.Transaction.Id == transaction.Id).ToList();
            foreach (var item in items)
            {
                if (item.Value.Operation == "M")
                {
                    var userFromdb = await context.Users
                    .FirstOrDefaultAsync(
                        x => x.Id == item.Value.User.Id);
                    item.Value.User = await context.SaveUserAsync(
                        item.Value.User, userFromdb);
                }
                if (item.Value.Operation == "A")
                {
                    var user = await context.SaveUserAsync(
                        item.Value.User, null);
                    newIds.Add(item.Value.User.Id, user.Id);
                    item.Value.User = user;
                }
                if (item.Value.Operation == "D")
                {
                    var result = await context.DeleteUserAsync(
                        item.Value.User);
                }
            }
            return true;
        }

        public async Task<User> GetUserByUserName(string username)
        {
            using (var context = new HelloContext(connectionString))
            {
                return await context.Users.FirstOrDefaultAsync(x => x.Username == username);
            }
        }


    }
}

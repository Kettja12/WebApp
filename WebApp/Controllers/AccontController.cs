using DBContext;
using Services;
using Services.Extenders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    public class AccontController
    {
        public List<User> CacheUsers { get; private set; }
        public readonly SessionServices services = SessionServices.GetInstance;
        private User currentUser;
        private readonly string sessionID;

        public AccontController(string sessionID)
        {
            this.sessionID = sessionID;
        }

        public async Task LoadUsersData()
        {
            CacheUsers = await services.GetUsersAsync();
        }

        public async Task<User> GetCurrentUser(string username)
        {
            currentUser = await services.GetUserByUserName(username);
            return currentUser;
        }

        public string TransactionCount
        {
            get { return services.GetSessionTransactionCount(sessionID).ToString(); }
        }
        public async Task<string> SaveUserAsync(
            string sId,
            string username,
            string firstName,
            string lastName)
        {
            if (currentUser == null)
            {
                return "Käyttäjätietoa ei valittu";
            }
            if (int.TryParse(sId, out int id) == false)
            {
                return "käyttäjä id virheellinen";
            }
            foreach (var user in CacheUsers)
            {
                if (user.Id == id)
                {
                    var user2 = await services.GetUserByUserName(username);
                    if (user2 != null && user.Id != user2.Id)
                    {
                        return "Käyttäjätunnus käytössä jo toisella käyttäjällä";
                    }
                    var saveUser = new User()
                    {
                        Id = id,
                        Username = username,
                        FirstName = firstName,
                        LastName = lastName
                    };
                    var clone = user.Clone();
                    if (clone.IsModified(saveUser))
                    {
                        var transaction = services.StartTransaction(
                                sessionID,
                                currentUser.Username);
                        if (transaction != null)
                        {
                            var result = services.SetUser(transaction, clone);
                            if (result != "")
                            {
                                services.RollbackTransaction(transaction);
                            }
                            return result;
                        }
                        else
                        {
                            return "Muutoksen luonti virhe";
                        }
                    }
                    else
                    {
                        return "Tietoja ei muutettu";
                    }
                }
            }
            return string.Empty;
        }

        public string DeleteUser(
            string sId)
        {
            if (currentUser == null)
            {
                return "Käyttäjätietoa ei valittu";
            }
            if (int.TryParse(sId, out int id) == false)
            {
                return "Virhe käyttäjän valinnassa";
            }
            foreach (var user in CacheUsers)
            {
                if (user.Id == id)
                {
                    var clone = user.Clone();
                    var transaction = services.StartTransaction(
                        sessionID,
                        currentUser.Username);
                    if (transaction != null)
                    {
                        var result = services.DeleteUser(transaction, clone);
                        if (result != "")
                        {
                            services.RollbackTransaction(transaction);
                        }
                        return result;
                    }
                    else
                    {
                        return "Transaction luonti virhe";
                    }
                }
            }
            return string.Empty;
        }

        public string AddUserClaim(
            string sid,
            string claimValue)
        {
            if (int.TryParse(sid, out int id))
            {
                var user = CacheUsers.FirstOrDefault(x => x.Id == id);
                if (user == null)
                {
                    return "Muutoksen luonti virhe";
                }
                var clone = user.Clone();
                var transaction = services.StartTransaction(
                    sessionID,
                    currentUser.Username);
                if (transaction != null)
                {
                    clone.Claims.Add(new Claim()
                    {
                        Id = services.GetAddId(),
                        UserId = clone.Id,
                        ClaimType = "uusi",
                        ClaimValue = claimValue
                    });
                    var result = services.SetUser(transaction, clone);
                    if (result != "")
                    {
                        services.RollbackTransaction(transaction);
                    }
                    return result;
                }
            }
            return "Muutoksen luonti virhe";
        }


        public async Task<string> AddUserAsync(
            string sId,
            string username,
            string firstName,
            string lastName)
        {
            if (currentUser == null)
            {
                return "Käyttäjätietoa ei valittu";
            }
            User newUser = null;
            if (int.TryParse(sId, out int id))
            {
                foreach (var user in CacheUsers)
                {
                    if (user.Id == id)
                    {
                        newUser = user.Clone();
                        newUser.Username = "";
                        break;
                    }
                }
            }
            if (newUser == null)
            {
                var user = await services.GetUserByUserName(username);
                if (user != null)
                {
                    return "Käyttäjätunnus jo käytössä";
                }
                newUser = new User()
                {
                    Username = username,
                    FirstName = firstName,
                    LastName = lastName
                };
            }
            var transaction = services.StartTransaction(
                sessionID,
                currentUser.Username);
            if (transaction == null)
            {
                return "Muutoksen luonti virhe";
            }
            var result = services.AddUser(transaction, newUser);
            if (result != "")
            {
                services.RollbackTransaction(transaction);
            }
            return result;

        }
        public async Task<string> SavetoDbAllTransactionsAsync()
        {

            if (await services.SavetoDbAllTransactionsAsync(sessionID))
            {
                return "Muutosten tallennus kantaan onnistui";
            }
            else
            {
                return "Muutosten tallennus kantaan epäonnistui";
            }

        }

        public string UndoAllTransaction()
        {
            if (services.UndoAllTransaction(sessionID))
            {
                return "Muutosten peruminen onnistui";
            }
            else
            {
                return "Muutosten peruminen epäonnistui";
            }

        }

        public string UndoTransaction()
        {
            if (services.UndoTransaction(sessionID))
            {
                return "Muutoksen peruminen onnistui";
            }
            else
            {
                return "Muutoksen peruminen epäonnistui";
            }

        }

    }
}
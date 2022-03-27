using DBContext;
using Services;
using Services.Extenders;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApp.Controllers
{
    /// <summary>
    /// Luokka jolla ylläpidetään käyttäjään liittyviä tietoja.
    /// Ja tarjotaan niitä käyttöliittymä luokalle.
    /// voisi olla muitakin rakanteita joita käsitellään samalla
    /// Jokainen aspx-sivun ei vältämätä tarvitse omaa kontrollia mutta 
    /// Karkesti yleistäen on kuitenkin MVC mallista  M ja C
    /// </summary>
    public class AccontController
    {
        // Data voidaan hakea kannasta jokakerta tai sitten käyttää 
        // sisäsitä  cache rakennetta. että tieto on vain  tietyn ajan hetken kata tietoo. 
        public List<User> CacheUsers { get; private set; }
        // Tässä rakenteessa on perustieto kaikien käyttäjien tekemistä muutoksita
        // joita ei ole tallennettu kantaan. Huomaa Singelton toteutus.
        public readonly SessionServices services = SessionServices.GetInstance;
        //Koska rakenne on sessiokohtainen  niin  kun käyttäjä tieto pitää kerrna asettaa.
        //Ei tehdä sitä erikseen session vain  luokansisään. JOs toinen controller luokka  tarvitsee tietoa
        //Se voi kaivaa sessiosta AccontController luokan kja kaivaa tiedosot siitä 
        private User currentUser;
        // Kaikki luokan muutokset menee sesssioID muuttujan taakse.
        private readonly string sessionID;

        public AccontController(string sessionID)
        {
            this.sessionID = sessionID;
        }
        /// <summary>
        /// Käytttäjätiedot hataan services instansin kautta.
        /// </summary>
        /// <returns></returns>
        public async Task LoadUsersData()
        {
            CacheUsers = await services.GetUsersAsync();
        }

        /// <summary>
        /// Ykisttäisen  käyttäjän tietojen haki ja samalla asetetaan käyttäjätiedot kuikaan 
        /// currenUser muutujaan.
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public async Task<User> GetCurrentUser(string username)
        {
            currentUser = await services.GetUserByUserName(username);
            return currentUser;
        }

        /// <summary>
        /// Muutosten määrä haetaan services luokasta.
        /// </summary>
        public string TransactionCount
        {
            get { return services.GetSessionTransactionCount(sessionID).ToString(); }
        }
        /// <summary>
        /// Köyttäjän tietojen muutoksen tallennus käyttöliittymästä
        /// </summary>
        /// <param name="sId"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
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
            //Kaivetaan käyttäjä cachehessä. Kun on muutos on oltava olemassa.
            foreach (var user in CacheUsers)
            {
                if (user.Id == id)
                {
                    // Pientä tarksitusta jos käyttäjätunnusta on muutettu pitää tarksitaa
                    // Onko muutettu tunnus käytössä
                    var user2 = await services.GetUserByUserName(username);
                    if (user2 != null && user.Id != user2.Id)
                    {
                        return "Käyttäjätunnus käytössä jo toisella käyttäjällä";
                    }
                    // Luodaan käyttäliittumän tiedoista  uusi käyttäjä vertailua varten.
                    var saveUser = new User()
                    {
                        Id = id,
                        Username = username,
                        FirstName = firstName,
                        LastName = lastName
                    };
                    // Tehdäään kopio aikaisemmista tiedoista.
                    var clone = user.Clone();
                    //Tallenetaan kopionn muuttuneet tiedot ja jos tiedot on oikeasti muutuneet 
                    // niin mennään lohkoon
                    if (clone.IsModified(saveUser))
                    {
                        // Luodaan transactio joka on muutoksen juutisolu johon sitten
                        // liitetään varsinaiset muutokset 
                        var transaction = services.StartTransaction(
                                sessionID,
                                currentUser.Username);
                        if (transaction != null)
                        {
                            //Lisätään käyttäjä muutos transalcvioon ja jos se epäonnistuu perutaan koko transactio
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

        /// <summary>
        /// Käyttäjän poisto oma transactionsa tarvitaan vain käyttäjä id
        /// </summary>
        /// <param name="sId"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Käyttäjän claims alitauluun tähtävä muutos  on periaattessa käyttäjä-taulun transactio
        /// Jöhon otetaan kopio viimeisimmästä käyttäjätiedoista ja lisätään sen  claimseihin rivi.
        /// </summary>
        /// <param name="sid"></param>
        /// <param name="claimValue"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Käyttäjätietojen lisäys haetaan pohjaksi 
        /// jos  parametrina saadaan käyttäjän ID 
        /// </summary>
        /// <param name="sId"></param>
        /// <param name="username"></param>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
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
            // käytetään add metodia ei set metodia jota käytettiin muutos tilanteessa.
            var result = services.AddUser(transaction, newUser);
            if (result != "")
            {
                services.RollbackTransaction(transaction);
            }
            return result;

        }

        /// <summary>
        /// Kaikkien session transactionden tallennus kantaan
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Kaikkien session transactioden peruminen.
        /// </summary>
        /// <returns></returns>
        public string UndoAllTransaction()
        {
            if (services.UndoAllTransactions(sessionID))
            {
                return "Muutosten peruminen onnistui";
            }
            else
            {
                return "Muutosten peruminen epäonnistui";
            }

        }
        /// <summary>
        /// Viimeisimmän transaction periminen
        /// </summary>
        /// <returns></returns>
        public string UndoTransaction()
        {
            if (services.UndoLastTransaction(sessionID))
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
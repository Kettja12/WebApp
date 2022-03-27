using DBContext;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using WebApp.Controllers;

namespace WebApp
{
    public partial class index : Page
    {
        /// <summary>
        /// Sivuun liityvät toiminnot ja jäyttäjään liityvien istunnon tietojen hallinta
        /// hoideltaan AccontController luokan instanssiisa joka tallennetaan session.
        /// </summary>
        AccontController controller
        {
            get
            {
                if (Session["AccontController"] == null)
                {
                    Session["AccontController"] = new AccontController(
                        Session.SessionID);
                }
                return (AccontController)Session["AccontController"];

            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Ladataan nyttön data tähtävä näin koska page_lad metodia ei voida muuttaa
                //asyncroniseksi
                RegisterAsyncTask(new PageAsyncTask(LoadUsersData));
            }
        }

        /// <summary>
        /// Töytetään käyttäliitymän  käyttäjä lista contolleri luokasta saatavalla datalla
        /// </summary>
        /// <returns></returns>
        public async Task LoadUsersData()
        {
            await controller.LoadUsersData();
            ListBoxUsers.Items.Clear();
            foreach (User user in controller.CacheUsers)
            {
                var item = new ListItem()
                {
                    Text = user.Username,
                    Value = user.Id.ToString()
                };
                ListBoxUsers.Items.Add(item);
            }
        }

        protected void ListBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Ladataan valittu käyttäjä saamalla tulee mahdollisesi tieto
                // jos joku toinen käyttäjä on muokannut tietoja
                if (int.TryParse(ListBoxUsers.SelectedValue, out int id))
                {
                    var user = controller.CacheUsers.FirstOrDefault(x => x.Id == id);
                    if (user != null)
                    {
                        TextBoxId.Text = user.Id.ToString();
                        TextBoxUsername.Text = user.Username;
                        TextBoxFirstName.Text = user.FirstName;
                        TextBoxLastname.Text = user.LastName;
                        var modifiedUser = controller.services.IsUserModified(user.Id);
                        if (modifiedUser != null)
                        {
                            LabelModified.Text = "* Muuttaja " +
                                modifiedUser.Transaction.Username +
                                " tietoa ei tallennettu";
                        }
                        else
                        {
                            LabelModified.Text = "";
                        }
                    }
                    LabelErrorMessage.Text = "";
                    //Ladataan  käytäjään liityvä alitaulu claims omaan listaasa
                    LoadClaims(user);
                }
                ButtonUndo.Text = " peru viimeisin (" + controller.TransactionCount + ")";

            }
            catch (Exception ex)
            {
                LabelErrorMessage.Text = ex.Message;
            }
        }

        //Clams taulun tiedot löytyy alkuperäisen haun yhteydessä  palautetusta alitaulusta
        public void LoadClaims(User user)
        {
            ListBoxClaims.Items.Clear();
            foreach (var claim in user.Claims)
            {
                var item = new ListItem()
                {
                    Text = claim.ClaimType + " " + claim.ClaimValue,
                    Value = claim.ClaimType
                };
                ListBoxClaims.Items.Add(item);
            }
        }

        protected async void ButtonRefresh_ClickAsync(object sender, EventArgs e)
        {
            await Refresh();
        }

        protected async Task Refresh()
        {
            await LoadUsersData();
            ButtonUndo.Text = " peru viimeisin (" + controller.TransactionCount + ")";
        }
        /// <summary>
        /// Käyttäjän tietojan tallennus näytöltä cahheen.  sen jälkeen tiedot  näkyvät
        /// Seuraavalle käyttäjälle kun  käyttäjä  päivtää oman näyttönsä.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonSave_ClickAsync(object sender, EventArgs e)
        {

            LabelErrorMessage.Text = await controller
                .SaveUserAsync(
                    TextBoxId.Text,
                    TextBoxUsername.Text,
                    TextBoxFirstName.Text,
                    TextBoxLastname.Text);

            await Refresh();
        }

        protected async void ButtonDelete_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = controller.DeleteUser(TextBoxId.Text);
            ButtonClear_Click(sender, e);
            await Refresh();
        }

        /// <summary>
        /// Muutoksia voidaan perua yksikerrallaan tai sitten kaikki kerralla.
        /// Tässä tehnään viimeisimmän session tehty muutos.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonUndo_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = controller.UndoTransaction();
            await Refresh();


        }
        /// <summary>
        /// Muutoksia voidaan perua yksikerrallaan tai sitten kaikki kerralla
        /// tässä tednään kaikkein käynnissä olevan session muutokset
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonUndoAll_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = controller.UndoAllTransaction();
            await Refresh();

        }

        /// <summary>
        /// Haataan käyttäjä tieto tieto tallennetaan myös controlleriin josta sitten 
        /// päästää  käsiksi  käyttäjä tietoihin koodissa.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonGetCurrenUser_ClickAsync(object sender, EventArgs e)
        {
            var user = await controller.GetCurrentUser(TextBoxCurrentUser.Text);
            if (user != null)
            {
                LabelCurrentUser.Text = user.FirstName + " " + user.LastName;
            }
        }

        /// <summary>
        /// Tässä tehdään näytöllä olevista tiedoista  uusi käyttäjä
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonAdd_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = await controller.AddUserAsync(
                TextBoxId.Text,
                TextBoxUsername.Text,
                TextBoxFirstName.Text,
                TextBoxLastname.Text);

            await Refresh();
        }
        protected  void ButtonClear_Click(object sender, EventArgs e)
        {
            ListBoxUsers.SelectedIndex = -1;
            TextBoxId.Text = "";
            TextBoxUsername.Text = "";
            TextBoxFirstName.Text = "";
            TextBoxLastname.Text = "";
            ListBoxClaims.Items.Clear();
        }
        /// <summary>
        /// Käyttäjän sessiossa olevien muutostan tallennus tietojantaan 
        /// tapahtuu tässä olevalal kutsulla 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonSaveToDb_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = await controller.SavetoDbAllTransactionsAsync();
            await Refresh();

        }
        /// <summary>
        /// Claims alitauluun tehty muutkesta syntyy samanlainen oma transactio. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected async void ButtonAddClaim_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = controller.AddUserClaim(TextBoxId.Text,TextBoxClaim.Text);
            TextBoxClaim.Text = "";
            await Refresh();
        }



    }
}
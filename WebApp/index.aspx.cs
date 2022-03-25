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
                RegisterAsyncTask(new PageAsyncTask(LoadUsersData));
            }
        }

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
                    LoadClaims(user);
                }
                ButtonUndo.Text = " peru viimeisin (" + controller.TransactionCount + ")";

            }
            catch (Exception ex)
            {
                LabelErrorMessage.Text = ex.Message;
            }
        }
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
            await Refresh();
        }


        protected async void ButtonUndo_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = controller.UndoTransaction();
            await Refresh();


        }
        protected async void ButtonUndoAll_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = controller.UndoAllTransaction();
            await Refresh();

        }

        protected async void ButtonGetCurrenUser_ClickAsync(object sender, EventArgs e)
        {
            var user = await controller.GetCurrentUser(TextBoxCurrentUser.Text);
            if (user != null)
            {
                LabelCurrentUser.Text = user.FirstName + " " + user.LastName;
            }
        }
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

        protected async void ButtonSaveToDb_ClickAsync(object sender, EventArgs e)
        {
            LabelErrorMessage.Text = await controller.SavetoDbAllTransactionsAsync();
            await Refresh();

        }

    }
}
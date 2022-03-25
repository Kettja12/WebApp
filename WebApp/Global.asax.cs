using Services;
using System;
using System.Web;

namespace WebApp
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString;
            SessionServices.GetInstance.SetConnectionString(connectionString);
        }
        void Session_Start(object sender, EventArgs e)
        {
            Session.Timeout = 10;
        }
        void Session_End(object sender, EventArgs e)
        {
            var services = SessionServices.GetInstance;
            services.UndoAllTransaction(Session.SessionID);
        }
    }
}
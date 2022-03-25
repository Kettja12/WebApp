
namespace Services
{
    public class Transaction
    {
        public int Id { get; set; } 
        public string SessionID { get; set; }
        public string Username { get; set; }
        public Transaction(int id, string sessionID, string username)
        {
            this.Id = id;
            SessionID = sessionID;
            Username = username;
        }
    }
}
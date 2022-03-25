using DBContext;
namespace Services.Models
{
    public class TRUser
    {
        public TRUser(Transaction transaction,User user, string operation)
        {
            Transaction = transaction;
            User = user;
            Operation = operation;
        }
        public string Operation { get; set; }
        public User User { get; set; }
        public Transaction Transaction { get; set; }
    }
}

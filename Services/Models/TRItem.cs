using DBContext;
namespace Services.Models
{
    public class TRItem
    {
        public TRItem(Transaction transaction,User user)
        {
            Transaction = transaction;
            User = user;
        }
        public User User { get; set; }
        public Transaction Transaction { get; set; }
    }
}

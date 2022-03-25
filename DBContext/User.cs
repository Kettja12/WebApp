using System.Collections.Generic;

namespace DBContext
{

    public class User
    {
        public User()
        {
            Claims = new HashSet<Claim>();
        }

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }

        public virtual ICollection<Claim> Claims { get; set; }
    }
}

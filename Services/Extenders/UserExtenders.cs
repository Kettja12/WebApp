using DBContext;
namespace Services
{
    public static class UserExtenders
    {
        public static bool IsModified(this User user,User compare)
        {
            bool modified = false;
            if (user.Username != compare.Username)
            {
                user.Username = compare.Username;
                modified = true;
            }
            if (user.FirstName != compare.FirstName)
            {
                user.FirstName = compare.FirstName;
                modified = true;
            }
            if (user.LastName != compare.LastName)
            {
                user.LastName = compare.LastName;
                modified = true;
            }
            return modified;
        }
    }
}
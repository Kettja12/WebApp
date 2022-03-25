using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DBContext
{
    public partial class HelloContext
    {
        public async Task<User> SaveUserAsync(User user, User existingUser)
        {

            if (user.Id < 0)
            {
                user.Id = 0;
                Entry(user).State = EntityState.Added;
            }
            else
            {
                if (existingUser != null)
                {
                    Entry(existingUser).State = EntityState.Detached;
                }
                Entry(user).State = EntityState.Modified;
            }
            await SaveChangesAsync();
            foreach (var claim in user.Claims)
            {
                claim.UserId = user.Id;
                Claim newclaim = await SaveClaimAsync(
                    claim);
            }
            return user;
        }

        public async Task<Claim> SaveClaimAsync(Claim claim)
        {
            if (claim != null)
            {
                if (claim.Id > 0)
                {
                    Entry(claim).State = EntityState.Detached;
                    Entry(claim).State = EntityState.Modified;

                    //Claim oldclaim = await Claims.FirstOrDefaultAsync(x => x.Id == claim.Id);
                    //if (oldclaim!= null)
                    //{
                    //    oldclaim.ClaimValue = claim.ClaimValue;
                    //}
                }
                else
                {
                    Entry(claim).State = EntityState.Added;
                    //await AddAsync(claim);
                }

                await SaveChangesAsync();
                return claim;
            }
            return new Claim();
        }

        public async Task<int> DeleteUserAsync(User user)
        {

            foreach (var claim in user.Claims)
            {
                Entry(claim).State = EntityState.Deleted;
            }
            Entry(user).State = EntityState.Deleted;
            int result = await SaveChangesAsync();
            return result;
        }

    }
}

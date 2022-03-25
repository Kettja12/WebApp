using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace DBContext
{
    public partial class HelloContext : DbContext
    {
        private readonly string connectionString;

        public HelloContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(connectionString);
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Claim> Claims => Set<Claim>();

        public async Task<User> GetUserByUsernameAsync(string username)
        {
            User user = await Users
                .FirstOrDefaultAsync(x => x.Username == username);
            return user;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Claim>(entity =>
            {
                entity.HasIndex(e => e.UserId);

                entity.Property(e => e.ClaimType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ClaimValue)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Claims)
                    .HasForeignKey(d => d.UserId);
            });

        }
    }
}

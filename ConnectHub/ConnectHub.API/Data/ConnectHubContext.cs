using Microsoft.EntityFrameworkCore;
using ConnectHub.Shared.Models;

namespace ConnectHub.API.Data
{
    public class ConnectHubContext : DbContext
    {
        public ConnectHubContext(DbContextOptions<ConnectHubContext> options) : base(options) {}

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<PrivateMessage> PrivateMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Post>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(p => p.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(c => c.UserId);

            modelBuilder.Entity<Comment>()
                .HasOne<Post>()
                .WithMany(p => p.Comments)
                .HasForeignKey(c => c.PostId);

            modelBuilder.Entity<PrivateMessage>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(pm => pm.SenderId);

            modelBuilder.Entity<PrivateMessage>()
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(pm => pm.ReceiverId);
        }
    }
}

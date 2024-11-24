using Microsoft.EntityFrameworkCore;
using ConnectHub.Shared.Models;

namespace ConnectHub.API.Data
{
    public class ConnectHubContext : DbContext
    {
        public ConnectHubContext(DbContextOptions<ConnectHubContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.User)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // User followers/following relationships
            modelBuilder.Entity<User>()
                .HasMany(u => u.Followers)
                .WithMany(u => u.Following)
                .UsingEntity(j => j.ToTable("UserFollows"));

            // Post relationships
            modelBuilder.Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Post>()
                .HasMany(p => p.LikedBy)
                .WithMany()
                .UsingEntity(j => j.ToTable("PostLikes"));

            // Comment relationships
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.LikedBy)
                .WithMany()
                .UsingEntity(j => j.ToTable("CommentLikes"));

            // Chat message relationships
            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ChatMessage>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Beauvoir.Models;

namespace Beauvoir.Models
{
    public  partial class AppDbContext : DbContext
    {
        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Model> Models { get; set; } = null!;
        public virtual DbSet<Tag> Tags { get; set; } = null!;
        public virtual DbSet<ModelTag> ModelTags { get; set; } = null!;
        public virtual DbSet<Friendship> Friendships { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PwdHash).HasMaxLength(256).IsRequired();
                entity.Property(e => e.PwdSalt).HasMaxLength(256).IsRequired();
                entity.Property(e => e.FirstName).HasMaxLength(256);
                entity.Property(e => e.LastName).HasMaxLength(256);
                entity.Property(e => e.Email).HasMaxLength(256);
            });

            modelBuilder.Entity<Model>(entity =>
            {
                entity.ToTable("Model");

                entity.Property(e => e.Name).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(e => e.Owner)
                      .WithMany(u => u.Models)
                      .HasForeignKey(e => e.OwnerId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Tag>(entity =>
            {
                entity.ToTable("Tag");

                entity.Property(e => e.Name).HasMaxLength(128).IsRequired();
            });

            modelBuilder.Entity<ModelTag>(entity =>
            {
                entity.ToTable("ModelTag");

                entity.HasKey(e => e.Id);

                entity.HasOne(mt => mt.Model)
                      .WithMany(m => m.ModelTags)
                      .HasForeignKey(mt => mt.ModelId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(mt => mt.Tag)
                      .WithMany(t => t.ModelTags)
                      .HasForeignKey(mt => mt.TagId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Friendship>(entity =>
            {
                entity.HasOne<User>()
                      .WithMany() // O .WithMany(u => u.FriendRequestsReceived) si lo defines en User
                      .HasForeignKey(f => f.ReceiverId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<User>()
                      .WithMany() // O .WithMany(u => u.FriendRequestsSent)
                      .HasForeignKey(f => f.RequesterId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}


using HasbitFlowApi.Models;
using Microsoft.EntityFrameworkCore;

namespace HasbitFlowApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Habit> Habits { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
                {
                    entity.HasKey(u => u.Id);

                    entity.Property(u => u.Name)
                    .IsRequired()
                    .HasMaxLength(100);

                    entity.Property(u => u.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                    entity.HasIndex(u => u.Email)
                    .IsUnique();

                    entity.Property(u => u.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                    entity.Property(u => u.CreatedAt)
                    .IsRequired();
                });


            modelBuilder.Entity<Habit>(entity =>
            {
                entity.HasKey(h => h.Id);

                entity.Property(h => h.Title)
                .IsRequired()
                .HasMaxLength(100);

                entity.Property(h => h.Description)
                .HasMaxLength(500);

                entity.Property(h => h.CreatedAt)
                .IsRequired();

                entity.Property(h => h.IsActive)
                .IsRequired();

                entity.HasOne(h => h.User)
                .WithMany(u => u.Habits)
                .HasForeignKey(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

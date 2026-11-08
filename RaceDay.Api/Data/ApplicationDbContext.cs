using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Enrolment> Enrolments => Set<Enrolment>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent a participant from enrolling
            modelBuilder.Entity<Enrolment>()
                .HasIndex(e => new { e.UserId, e.CategoryId })
                .IsUnique();

            // Seed Roles matching Part 1 script
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Organiser" },
                new Role { RoleId = 2, RoleName = "Participant" }
            );

            // between Users and Enrolments
            modelBuilder.Entity<Enrolment>()
                .HasOne(e => e.User)
                .WithMany(u => u.Enrolments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            // between Users and Results
            modelBuilder.Entity<Result>()
                .HasOne(r => r.RecordedByUser)
                .WithMany()
                .HasForeignKey(r => r.RecordedByUserId)
                .OnDelete(DeleteBehavior.NoAction);


            // Store race distances with two decimal places.
            modelBuilder.Entity<Category>()
                .Property(c => c.DistanceKm)
                .HasPrecision(18, 2);

            // Store entry fees accurately.
            modelBuilder.Entity<Category>()
                .Property(c => c.EntryFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Result>()
                .HasIndex(r => r.EnrolmentId)
                .IsUnique();


        }
    }
}
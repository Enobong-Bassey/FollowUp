using ChurchHospitalityApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChurchHospitalityApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Guest> Guests => Set<Guest>();
        public DbSet<FollowUp> FollowUps => Set<FollowUp>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<FollowUp>()
                .HasOne(f => f.Guest)
                .WithMany(g => g.FollowUps)
                .HasForeignKey(f => f.GuestId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FollowUp>()
                .HasOne(f => f.AssignedToUser)
                .WithMany(u => u.AssignedFollowUps)
                .HasForeignKey(f => f.AssignedToUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // A guest has at most one week-1 and one week-2 follow-up
            builder.Entity<FollowUp>()
                .HasIndex(f => new { f.GuestId, f.WeekNumber })
                .IsUnique();
        }
    }
}

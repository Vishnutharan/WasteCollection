using CleanLanka.Backend.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CleanLanka.Backend.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<WasteRequest> WasteRequests { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<CleanLanka.Backend.Models.Route> Routes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Activity> Activities { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<WasteRequest>()
                .HasOne(w => w.Citizen)
                .WithMany()
                .HasForeignKey(w => w.CitizenId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<CleanLanka.Backend.Models.Route>()
                .HasOne(r => r.Vehicle)
                .WithMany()
                .HasForeignKey(r => r.VehicleId);
        }
    }
}

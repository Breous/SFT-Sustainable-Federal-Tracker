using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SFT.Models;
using System.Linq;

namespace SFT.Data
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Purchase> Purchases { get; set; }

        // --- THE DATA GUARD: IMMUTABILITY OVERRIDE ---
        public override int SaveChanges()
        {
            // Detect any 'Purchase' entries that are being updated (modified)
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Purchase && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                // PROTECT THE AUDIT TRAIL
                // These fields are 'locked' after the initial creation.
                // Even if the code tries to change them, the DB will ignore the update.
                entry.Property("AuditStamp").IsModified = false;
                entry.Property("Date").IsModified = false;
                entry.Property("UserId").IsModified = false;
            }

            return base.SaveChanges();
        }

        // Standard Async version of the override
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is Purchase && e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                entry.Property("AuditStamp").IsModified = false;
                entry.Property("Date").IsModified = false;
                entry.Property("UserId").IsModified = false;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
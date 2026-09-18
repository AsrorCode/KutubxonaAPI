using KutubxonaAPI.Models;
using KutubxonaAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KutubxonaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ===== DbSet lar =====
    public DbSet<User> Users { get; set; }
    public DbSet<SaleBook> SaleBooks { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<Collection> Collections { get; set; }
    public DbSet<CollectionItem> CollectionItems { get; set; }
    public DbSet<WishlistItem> WishlistItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== User =====
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Enum'lar STRING sifatida saqlanadi
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>()
            .HasMaxLength(30);

        modelBuilder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        // ===== Decimal precision (pul) =====
        modelBuilder.Entity<SaleBook>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);

        modelBuilder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(18, 2);

        modelBuilder.Entity<OrderItem>()
            .Property(oi => oi.PriceAtOrder)
            .HasPrecision(18, 2);

        // ===== Order → OrderItem (cascade) =====
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.Order)
            .WithMany(o => o.Items)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.SaleBook)
            .WithMany(s => s.OrderItems)
            .HasForeignKey(oi => oi.SaleBookId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Review → SaleBook / User =====
        modelBuilder.Entity<Review>()
            .HasOne(r => r.SaleBook)
            .WithMany()
            .HasForeignKey(r => r.SaleBookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Review>()
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Collection → Items → SaleBook =====
        modelBuilder.Entity<CollectionItem>()
            .HasOne(ci => ci.Collection)
            .WithMany(c => c.Items)
            .HasForeignKey(ci => ci.CollectionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollectionItem>()
            .HasOne(ci => ci.SaleBook)
            .WithMany()
            .HasForeignKey(ci => ci.SaleBookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollectionItem>()
            .HasIndex(ci => new { ci.CollectionId, ci.SaleBookId })
            .IsUnique();

        // ===== Wishlist =====
        modelBuilder.Entity<WishlistItem>()
            .HasOne(w => w.SaleBook)
            .WithMany()
            .HasForeignKey(w => w.SaleBookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WishlistItem>()
            .HasOne(w => w.User)
            .WithMany()
            .HasForeignKey(w => w.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<WishlistItem>()
            .HasIndex(w => new { w.UserId, w.SaleBookId })
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.Token)
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasIndex(rt => rt.UserId);

        // ============================================
        // GLOBAL QUERY FILTERS — Soft Delete
        // O'chirilgan yozuvlar avtomatik filtrlanadi
        // ============================================
        modelBuilder.Entity<SaleBook>().HasQueryFilter(s => !s.IsDeleted);
        modelBuilder.Entity<Order>().HasQueryFilter(o => !o.IsDeleted);
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);

        // ===== Indexes =====
        modelBuilder.Entity<SaleBook>().HasIndex(s => s.Category);
        modelBuilder.Entity<Review>().HasIndex(r => r.SaleBookId);
        modelBuilder.Entity<Notification>().HasIndex(n => n.CreatedAt);
        modelBuilder.Entity<SaleBook>().HasIndex(s => s.IsDeleted);
        modelBuilder.Entity<Order>().HasIndex(o => o.UserId);
        modelBuilder.Entity<Order>().HasIndex(o => o.Status);
    }

    // ============================================
    // SAVE CHANGES OVERRIDE — Soft Delete audit
    // Remove chaqirilsa — IsDeleted=true qilib qo'yamiz
    // ============================================
    public override int SaveChanges()
    {
        ApplySoftDelete();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplySoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplySoftDelete()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is ISoftDelete && e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            entry.State = EntityState.Modified;
            var entity = (ISoftDelete)entry.Entity;
            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            // DeletedByUserId — controller'da qo'yilishi mumkin
        }
    }
}

using KutubxonaAPI.Models;
using KutubxonaAPI.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KutubxonaAPI.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ===== DbSet lar =====
    public DbSet<Book> Books { get; set; }
    public DbSet<BookPage> BookPages { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<SaleBook> SaleBooks { get; set; }       // ← QO'SHILDI
    public DbSet<Order> Orders { get; set; }              // ← QO'SHILDI
    public DbSet<OrderItem> OrderItems { get; set; }      // ← QO'SHILDI
    public DbSet<RefreshToken> RefreshTokens { get; set; } // ← Refresh tokens

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== User =====
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Enum'lar STRING sifatida saqlanadi (o'qishga oson, migration'ga qulay)
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

        // ===== OrderItem → SaleBook (restrict) =====
        modelBuilder.Entity<OrderItem>()
            .HasOne(oi => oi.SaleBook)
            .WithMany(s => s.OrderItems)
            .HasForeignKey(oi => oi.SaleBookId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== Book → Pages (cascade) =====
        modelBuilder.Entity<BookPage>()
            .HasOne(p => p.Book)
            .WithMany(b => b.Pages)
            .HasForeignKey(p => p.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== Book → Comments (cascade) =====
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Book)
            .WithMany(b => b.Comments)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        // ===== User → Comments (restrict — user o'chirilsa comment qolsin) =====
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.User)
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== RefreshToken → User (cascade) =====
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

        // ===== Indexes (STEP 3 uchun ham foydali) =====
        modelBuilder.Entity<Book>().HasIndex(b => b.Category);
        modelBuilder.Entity<Book>().HasIndex(b => b.CreatedAt);
        modelBuilder.Entity<Comment>().HasIndex(c => c.BookId);
        modelBuilder.Entity<BookPage>().HasIndex(p => p.BookId);
        modelBuilder.Entity<SaleBook>().HasIndex(s => s.Category);
        modelBuilder.Entity<Order>().HasIndex(o => o.UserId);
        modelBuilder.Entity<Order>().HasIndex(o => o.Status);

        // ===== Seed Books =====
        modelBuilder.Entity<Book>().HasData(
            new Book
            {
                Id = 1,
                Title = "O'tkan kunlar",
                Author = "Abdulla Qodiriy",
                Year = 1925,
                Category = "Klassika",
                IsAvailable = true,
                CreatedAt = new DateTime(2024, 1, 1)
            },
            new Book
            {
                Id = 2,
                Title = "Mehrobdan chayon",
                Author = "Abdulla Qodiriy",
                Year = 1929,
                Category = "Klassika",
                IsAvailable = true,
                CreatedAt = new DateTime(2024, 1, 1)
            }
        );
    }
}
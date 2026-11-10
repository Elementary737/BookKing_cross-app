using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<BookRow> Books => Set<BookRow>();
    public DbSet<CopyRow> Copies => Set<CopyRow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var book = modelBuilder.Entity<BookRow>();
        book.HasKey(b => b.Id);
        book.Property(b => b.Id).HasMaxLength(32);
        book.Property(b => b.Isbn).IsRequired().HasMaxLength(32);
        book.Property(b => b.Title).IsRequired().HasMaxLength(200);
        book.Property(b => b.Year).IsRequired();
        book.Property(b => b.Author).HasMaxLength(100);
        book.Property(b => b.AvailableCopies).IsRequired();
        book.HasIndex(b => b.Isbn).IsUnique();

        book.HasMany(b => b.Copies)
            .WithOne(c => c.Book)
            .HasForeignKey(c => c.BookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CopyRow>(c =>
        {
            c.HasKey(x => x.Id);
            c.Property(x => x.InventoryNumber).IsRequired().HasMaxLength(50);
            c.Property(x => x.Condition).IsRequired().HasMaxLength(50);
        });
    }
}
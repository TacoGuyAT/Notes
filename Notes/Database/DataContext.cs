using Microsoft.EntityFrameworkCore;

namespace Notes.Database;

public class DataContext(DbContextOptions<DataContext> o) : DbContext(o) {
    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();

    protected override void OnModelCreating(ModelBuilder b) {
        b.Entity<User>().HasIndex(u => u.Username).IsUnique();
        b.Entity<Note>().HasIndex(d => d.Slug).IsUnique();
    }
}

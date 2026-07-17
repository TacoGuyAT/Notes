using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Notes.Database;

/// <summary>
/// Lets `dotnet ef` construct the context at design time without booting the web host. The
/// connection string only matters for commands that touch a database (e.g. database update);
/// override it with the ConnectionStrings__Notes env var to keep off the real notes.db.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DataContext> {
    public DataContext CreateDbContext(string[] args) {
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Notes") ?? "Data Source=notes.db";
        var options = new DbContextOptionsBuilder<DataContext>().UseSqlite(conn).Options;
        return new DataContext(options);
    }
}

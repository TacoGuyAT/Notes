using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Notes.Database;
using Notes.Endpoints;
using Notes.Extensions;
using Notes.Services;

namespace Notes;

public class Program {
    public static void Main(string[] args) {
        var builder = WebApplication.CreateBuilder(args);

        var pepper = builder.Configuration["NOTES_PEPPER"]
#if DEBUG
            ?? "NOTES_DEBUG";
#else
            ?? throw new InvalidOperationException("Set NOTES_PEPPER env var before starting.");
#endif

        builder.Services.AddDbContext<DataContext>(o =>
            o.UseSqlite(builder.Configuration.GetConnectionString("Notes") ?? "Data Source=notes.db"));
        builder.Services.AddSingleton(new PasswordHasher(pepper));
        builder.Services.AddSingleton<MarkdownRenderer>();

        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(o => {
                o.Events.OnRedirectToLogin = ctx => {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                };

                o.Events.OnValidatePrincipal = ValidateAccountStillExists;
            });
        builder.Services.AddAuthorization();

        var app = builder.Build();

        using (var scope = app.Services.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<DataContext>();
            db.Database.Migrate();
        }

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.MapAuthEndpoints();
        app.MapNotesEndpoints();
        app.MapPageEndpoints();

        app.Run();
    }

    private static async Task ValidateAccountStillExists(CookieValidatePrincipalContext ctx) {
        var db = ctx.HttpContext.RequestServices.GetRequiredService<DataContext>();

        var uid = ctx.Principal?.FindFirst(ClaimsPrincipalExtensions.UserIdClaim)?.Value;
        var stamp = ctx.Principal?.FindFirst(ClaimsPrincipalExtensions.SecurityStampClaim)?.Value;

        var ok = long.TryParse(uid, out var id)
                 && !string.IsNullOrEmpty(stamp)
                 && await db.Users.AnyAsync(u => u.Id == id && u.SecurityStamp == stamp);
        if (ok) return;

        ctx.RejectPrincipal();
        await ctx.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}

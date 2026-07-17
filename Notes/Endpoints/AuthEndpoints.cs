using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Notes.Database;
using Notes.Dtos;
using Notes.Extensions;
using Notes.Services;

namespace Notes.Endpoints;

public static class AuthEndpoints {
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app) {
        app.MapPost("/api/register", async (DataContext db, PasswordHasher hasher, Credentials c) => {
            if (string.IsNullOrWhiteSpace(c.Username) || string.IsNullOrWhiteSpace(c.Password))
                return Results.BadRequest("username/password required");
            if (c.Username.Length > Limits.Username)
                return Results.BadRequest($"username must be at most {Limits.Username} characters");
            if (c.Password.Length > Limits.Password)
                return Results.BadRequest($"password must be at most {Limits.Password} characters");
            if (await db.Users.AnyAsync(u => u.Username == c.Username))
                return Results.Conflict("username taken");

            var (hash, salt) = hasher.Hash(c.Password);
            db.Users.Add(new User {
                Username = c.Username,
                PasswordHash = hash,
                Salt = salt,
                SecurityStamp = User.NewStamp(),
            });
            try {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException) {
                // Two registrations for the same name can both pass the check above and race to
                // insert; the unique index is what actually decides, so report its verdict.
                return Results.Conflict("username taken");
            }
            return Results.Ok();
        });

        app.MapPost("/api/login", async (HttpContext ctx, DataContext db, PasswordHasher hasher, Credentials c) => {
            if (c.Password is null || c.Password.Length > Limits.Password) return Results.Unauthorized();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == c.Username);
            if (user is null || !hasher.Verify(c.Password, user.PasswordHash, user.Salt))
                return Results.Unauthorized();

            var identity = new ClaimsIdentity([
                new Claim(ClaimsPrincipalExtensions.UserIdClaim, user.Id.ToString()),
                new Claim(ClaimsPrincipalExtensions.UserNameClaim, user.Username),
                new Claim(ClaimsPrincipalExtensions.SecurityStampClaim, user.SecurityStamp)
            ], CookieAuthenticationDefaults.AuthenticationScheme);
            await ctx.SignInAsync(new ClaimsPrincipal(identity));
            return Results.Ok();
        });

        app.MapPost("/api/logout", async (HttpContext ctx) => {
            await ctx.SignOutAsync();
            return Results.Ok();
        });

        app.MapGet("/api/me", (HttpContext ctx) =>
            ctx.User.Identity?.IsAuthenticated == true
                ? Results.Ok(new { username = ctx.User.UserName() })
                : Results.Unauthorized());

        return app;
    }
}

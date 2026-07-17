using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Notes.Database;
using Notes.Dtos;
using Notes.Extensions;
using Notes.Services;

namespace Notes.Endpoints;

public static class NotesEndpoints {
    public static IEndpointRouteBuilder MapNotesEndpoints(this IEndpointRouteBuilder app) {
        app.MapGet("/api/notes", async (HttpContext ctx, DataContext db) => {
            var uid = ctx.User.UserId();
            return await db.Notes.Where(d => d.UserId == uid)
                .OrderByDescending(d => d.Id)
                .Select(d => new { d.Id, d.Title, d.Public, d.Slug })
                .ToListAsync();
        }).RequireAuthorization();

        app.MapPost("/api/notes", async (HttpContext ctx, DataContext db, NewNote d) => {
            if (TooLong(d) is { } tooLong) return tooLong;

            var doc = new Note {
                UserId = ctx.User.UserId(),
                Title = d.Title ?? "Untitled",
                Content = d.Content ?? "",
                Slug = RandomSlug(),
            };
            db.Notes.Add(doc);
            await db.SaveChangesAsync();
            return Results.Ok(new { doc.Id, doc.Title, doc.Content, doc.Public, doc.Slug });
        }).RequireAuthorization();

        app.MapGet("/api/notes/{id:long}", async (HttpContext ctx, DataContext db, MarkdownRenderer markdown, long id) => {
            var uid = ctx.User.UserId();
            var doc = await db.Notes.FirstOrDefaultAsync(d => d.Id == id && d.UserId == uid);
            return doc is null
                ? Results.NotFound()
                : Results.Ok(new { doc.Id, doc.Title, doc.Content, doc.Public, doc.Slug, html = markdown.ToSafeHtml(doc.Content) });
        }).RequireAuthorization();

        app.MapPut("/api/notes/{id:long}", async (HttpContext ctx, DataContext db, MarkdownRenderer markdown, long id, NewNote d) => {
            if (TooLong(d) is { } tooLong) return tooLong;

            var uid = ctx.User.UserId();
            var doc = await db.Notes.FirstOrDefaultAsync(x => x.Id == id && x.UserId == uid);
            if (doc is null) return Results.NotFound();
            doc.Title = d.Title ?? "Untitled";
            doc.Content = d.Content ?? "";
            await db.SaveChangesAsync();
            return Results.Ok(new { doc.Id, doc.Title, doc.Public, doc.Slug, html = markdown.ToSafeHtml(doc.Content) });
        }).RequireAuthorization();

        app.MapPut("/api/notes/{id:long}/visibility", async (HttpContext ctx, DataContext db, long id, NoteVisibility v) => {
            var uid = ctx.User.UserId();
            var doc = await db.Notes.FirstOrDefaultAsync(d => d.Id == id && d.UserId == uid);
            if (doc is null) return Results.NotFound();
            doc.Public = v.Public;
            doc.Slug ??= RandomSlug();
            await db.SaveChangesAsync();
            return Results.Ok(new { doc.Public, doc.Slug });
        }).RequireAuthorization();

        app.MapGet("/api/public/{slug}", async (DataContext db, MarkdownRenderer markdown, string slug) => {
            var doc = await db.Notes.FirstOrDefaultAsync(d => d.Slug == slug && d.Public);
            if (doc is null) return Results.NotFound();
            return Results.Ok(new { doc.Title, html = markdown.ToSafeHtml(doc.Content) });
        });

        return app;
    }

    private static string RandomSlug() => Convert.ToHexString(RandomNumberGenerator.GetBytes(6)).ToLowerInvariant();

    private static IResult? TooLong(NewNote d) =>
        d.Title?.Length > Limits.Title ? Results.BadRequest($"title must be at most {Limits.Title} characters")
        : d.Content?.Length > Limits.Content ? Results.BadRequest($"content must be at most {Limits.Content} characters")
        : null;
}

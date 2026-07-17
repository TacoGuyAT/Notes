namespace Notes.Endpoints;

public static class PageEndpoints {
    public static IEndpointRouteBuilder MapPageEndpoints(this IEndpointRouteBuilder app) {
        app.MapGet("/editor", (IWebHostEnvironment env) =>
            Results.File(Path.Combine(env.WebRootPath, "editor.html"), "text/html"));

        app.MapGet("/{slug:regex(^[0-9a-f]{{12}}$)}", (IWebHostEnvironment env, string slug) =>
            Results.File(Path.Combine(env.WebRootPath, "view.html"), "text/html"));

        return app;
    }
}

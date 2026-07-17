using Ganss.Xss;
using Markdig;

namespace Notes.Services;

public class MarkdownRenderer(ILogger<MarkdownRenderer> log) {
    private readonly MarkdownPipeline _pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
    private readonly HtmlSanitizer _sanitizer = new();

    public string ToSafeHtml(string markdown) {
        try {
            return _sanitizer.Sanitize(Markdown.ToHtml(markdown, _pipeline));
        }
        catch (ArgumentException e) {
            // Markdig refuses pathologically nested input (e.g. thousands of stacked blockquotes)
            // rather than blowing the stack. That is a bad note, not a broken server: keep the
            // content, tell the reader it could not be rendered.
            log.LogWarning(e, "Markdown could not be rendered ({Length} chars)", markdown.Length);
            return "<p><em>This note could not be rendered because its markdown is nested too deeply.</em></p>";
        }
    }
}

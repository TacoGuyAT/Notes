namespace Notes.Database;

public class Note {
    public long Id { get; set; }
    public long UserId { get; set; }
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public bool Public { get; set; }
    public string? Slug { get; set; }

}
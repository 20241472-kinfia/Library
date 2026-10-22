namespace library.Models;

public class Note
{
    private DateOnly date;

    public long NoteId { get; set; }               // note_id         BIGINT, the primary key
    public string Title { get; set; } = "";        // title           VARCHAR(10) NOT NULL
    public string Body { get; set; } = "";         // text            VARCHAR(150) NOT NULL
    public DateOnly Date { get; set ; }
}




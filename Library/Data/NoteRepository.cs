using library.Models;
using Npgsql;

namespace library.Data;

public class NoteRepository
{
    private readonly NpgsqlDataSource _dataSource;

    // ASP.NET Core hands in the data source that Program.cs registered.
    public NoteRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Note.
    // The positions 0 to 3 match the column order in both SELECTs above.
    private static Note ReadNote(NpgsqlDataReader reader)
    {
        return new Note
        {
            NoteId = reader.GetInt64(0),
            Title = reader.GetString(1),
            Body = reader.IsDBNull(2) ? "" : reader.GetString(2),
            Date = reader.GetFieldValue<DateOnly>(3),
        };
    }

    // CREATE: insert a new note. The database chooses its note_id.
    public async Task AddAsync(Note note)
    {
        const string sql = "INSERT INTO lending.note ( title, body, created_on) " +
                           "VALUES (@title, @body, @date);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("title", note.Title);
        command.Parameters.AddWithValue("body", note.Body);
        command.Parameters.AddWithValue("date", note.Date);

        await command.ExecuteNonQueryAsync();
    }

    // READ: every note, sorted by name.
    public async Task<List<Note>> GetAllAsync()
    {
        const string sql = "SELECT note_id, title, body, created_on FROM lending.note ORDER BY note_id;";

        var notes = new List<Note>();

        // The command borrows a connection; 'await using' gives it back when the method ends.
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            notes.Add(ReadNote(reader));
        }

        return notes;
    }

    // READ: one note, or null when no note has that id.
    public async Task<Note?> GetByIdAsync(long id)
    {
        const string sql = "SELECT note_id, title, body, created_on FROM lending.note WHERE note_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadNote(reader) : null;
    }

    // UPDATE: change every column of one note.
    public async Task UpdateAsync(Note note)
    {
        const string sql = "UPDATE lending.note " +
                           "SET title = @title, body = @body, created_on = @date " +
                           "WHERE note_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", note.NoteId);
        command.Parameters.AddWithValue("title", note.Title);
        command.Parameters.AddWithValue("body", note.Body);
        command.Parameters.AddWithValue("date", note.Date);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one note.
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.note WHERE note_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }
}

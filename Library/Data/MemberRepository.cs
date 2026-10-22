using library.Models;
using Npgsql;

namespace library.Data;

// Every SQL statement about books lives in this one class.
public class MemberRepository
{
    private readonly NpgsqlDataSource _dataSource;

    // ASP.NET Core hands in the data source that Program.cs registered.
    public MemberRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    // Copies the current row of the reader into a Member.
    // The positions 0 to 3 match the column order in both SELECTs above.
    private static Member ReadMember(NpgsqlDataReader reader)
    {
        return new Member
        {
            MemberId = reader.GetInt64(0),
            Name = reader.GetString(1),
            Type = reader.GetString(2), 
            Email = reader.IsDBNull(3) ? null : reader.GetString(3),
            Date = reader.GetFieldValue<DateOnly>(4),
        };
    }

    // CREATE: insert a new member. The database chooses its member_id.
    public async Task AddAsync(Member member)
    {
        const string sql = "INSERT INTO lending.member (full_name, member_type, email) " +
                           "VALUES (@name, @type, @email);";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("name", member.Name);
        command.Parameters.AddWithValue("type", member.Type);
        // A C# null must be sent as DBNull.Value, which is how a database NULL is written.
        command.Parameters.AddWithValue("email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("join_date", member.Date);

        await command.ExecuteNonQueryAsync();           // runs SQL that returns no rows
    }

    // READ: every member, sorted by name.
    public async Task<List<Member>> GetAllAsync()
    {
        const string sql = "SELECT member_id, full_name, member_type, email, join_date FROM lending.member ORDER BY member_id;";

        var members = new List<Member>();

        // The command borrows a connection; 'await using' gives it back when the method ends.
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())        // once for each row
        {
            members.Add(ReadMember(reader));
        }

        return members;
    }

    // READ: one member, or null when no member has that id.
    public async Task<Member?> GetByIdAsync(long id)
    {
        const string sql = "SELECT member_id, full_name, member_type, email, join_date FROM lending.member WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);      // @id in the SQL receives this value

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? ReadMember(reader) : null;
    }

    // UPDATE: change every column of one member.
    public async Task UpdateAsync(Member member)
    {
        const string sql = "UPDATE lending.member " +
                           "SET full_name = @name, member_type = @type, email = @email , join_date = @date" +
                           "WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", member.MemberId);
        command.Parameters.AddWithValue("name", member.Name);
        command.Parameters.AddWithValue("type", member.Type);
        command.Parameters.AddWithValue("email", (object?)member.Email ?? DBNull.Value);
        command.Parameters.AddWithValue("join_date", member.Date);

        await command.ExecuteNonQueryAsync();
    }

    // DELETE: remove one member.
    public async Task DeleteAsync(long id)
    {
        const string sql = "DELETE FROM lending.member WHERE member_id = @id;";

        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", id);

        await command.ExecuteNonQueryAsync();
    }



}


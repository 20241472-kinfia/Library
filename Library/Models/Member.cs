namespace library.Models;

// One Book object holds one row of lending.book.
public class Member
{
    public long MemberId { get; set; }               // member_id           BIGINT, the primary key
    public string Name { get; set; } = "";           // full_name           VARCHAR(80) NOT NULL
    public string? Email { get; set; }               // email               VARCHAR(150) NOT NULL
    //public string Date { get; set; } = "";         // join_date           VARCHAR(150) NOT NULL
    public string Type { get; set; } = "";          // member_type         VARCHAR(10), may be NULL

}

using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("users")]
public class User
{
    [Column("id")]
    public long Id { get; set; }

    [Column("username")]
    public string Username { get; set; }

    [Column("enabled")]
    public bool? Enabled { get; set; }

    [Column("cin")]
    public string Cin { get; set; }

    [Column("last_name")]
    public string LastName { get; set; }

    [Column("first_name")]
    public string FirstName { get; set; }

    [Column("primary_email")]
    public string PrimaryEmail { get; set; }

    [Column("secondary_email")]
    public string SecondaryEmail { get; set; }

    [Column("temporary")]
    public bool Temporary { get; set; }
}

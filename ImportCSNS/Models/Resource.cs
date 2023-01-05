using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

public class ResourceType
{
    public const int None = 0;
    public const int Text = 1;
    public const int File = 2;
    public const int Url = 3;
}

[Table("resources")]
public class Resource
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("type")]
    public int Type { get; set; }

    [Column("text")]
    public string Text { get; set; }

    [Column("file_id")]
    public long? FileId { get; set; }
    public File File { get; set; }

    [Column("url")]
    public string Url { get; set; }

    [Column("private")]
    public bool Private { get; set; }

    [Column("deleted")]
    public bool Deleted { get; set; }
}

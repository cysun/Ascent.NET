using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("files")]
public class File
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("type")]
    public string Type { get; set; }

    [Column("size")]
    public long? Size { get; set; }

    [Column("date")]
    public DateTime? Date { get; set; }

    [Column("owner_id")]
    public long OwnerId { get; set; }

    [Column("public")]
    public bool Public { get; set; }

    [Column("folder")]
    public bool Folder { get; set; }

    [Column("parent_id")]
    public long? ParentId { get; set; }

    [Column("regular")]
    public bool Regular { get; set; }

    [Column("deleted")]
    public bool Deleted { get; set; }
}

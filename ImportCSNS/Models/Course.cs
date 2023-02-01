using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("courses")]
public class Course
{
    [Column("id")]
    public long Id { get; set; }

    [Column("code")]
    public string Code { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("units")]
    public int Units { get; set; }

    [Column("unit_factor")]
    public double UnitFactor { get; set; }

    [Column("obsolete")]
    public bool IsObsolete { get; set; }
}

[Table("sections")]
public class Section
{
    [Column("id")]
    public long Id { get; set; }

    [Column("term")]
    public int TermCode { get; set; }

    [Column("course_id")]
    public long CourseId { get; set; }
    public Course Course { get; set; }

    [Column("number")]
    public int Number { get; set; }

    [Column("deleted")]
    public bool IsDeleted { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("rubrics")]
public class Rubric
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("scale")]
    public int Scale { get; set; }

    public List<RubricIndicator> Indicators { get; set; } = new List<RubricIndicator>();

    [Column("public")]
    public bool IsPublic { get; set; }

    [Column("obsolete")]
    public bool IsOsolete { get; set; }

    [Column("deleted")]
    public bool IsDeleted { get; set; }
}

[Table("rubric_indicators")]
public class RubricIndicator
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("rubric_id")]
    public long RubricId { get; set; }
    public Rubric Rubric { get; set; }

    [Column("indicator_index")]
    public int Index { get; set; }

    public List<RubricIndicatorCriterion> Criteria { get; set; } = new List<RubricIndicatorCriterion>();
}

[Table("rubric_indicator_criteria")]
public class RubricIndicatorCriterion
{
    [Column("indicator_id")]
    public long RubricIndicatorId { get; set; }
    public RubricIndicator RubricIndicator { get; set; }

    [Column("criterion_index")]
    public int Index { get; set; }

    [Column("criterion")]
    public string Criterion { get; set; }
}

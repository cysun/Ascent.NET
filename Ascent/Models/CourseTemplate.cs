using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public class CourseTemplate
{
    public int Id { get; set; }

    public int CourseId { get; set; }
    public Course Course { get; set; }

    public List<AssignmentTemplate> AssignmentTemplates { get; set; }
}

public class AssignmentTemplate
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public string Description { get; set; }

    public int CourseTemplateId { get; set; }
    public CourseTemplate CourseTemplate { get; set; }

    public int? RubricId { get; set; }
    public Rubric Rubric { get; set; }

    public bool IsPeerReviewed { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

public static class QuestionType
{
    public const string Choice = "CHOICE";
    public const string Rating = "Rating";
    public const string Text = "TEXT";
}

public class QuestionSheet
{
    [Column("id")]
    public long Id { get; set; }

    [Column("description")]
    public string Description { get; set; }

    public List<QuestionSection> Sections { get; set; }

    public List<AnswerSheet> AnswerSheets { get; set; }
}

public class QuestionSection
{
    [Column("id")]
    public long Id { get; set; }

    [Column("description")]
    public string Description { get; set; }

    public List<Question> questions { get; set; }
}

public class Question
{
    [Column("id")]
    public long Id { get; set; }

    [Column("description")]
    public string Description { get; set; }

    [Column("point_value")]
    public int PointValue { get; set; }

    public List<Answer> Answers { get; set; }

    [Column("question_type")]
    public string Type { get; set; }

    // Choice Question Properties

    public List<QuestionChoice> Choices { get; set; }

    public List<QuestionCorrectSelection> CorrectSelections { get; set; }

    [Column("min_selections")]
    public int MinSelections { get; set; }

    [Column("max_selections")]
    public int MaxSelections { get; set; }

    // Rating Question Properties

    [Column("min_rating")]
    public int MinRating { get; set; }

    [Column("max_rating")]
    public int MaxRating { get; set; }

    // Text Question Properties

    [Column("correct_answer")]
    public string CorrectAnswer { get; set; }

    [Column("text_length")]
    public int TextLength { get; set; }
}

[Table("question_choices")]
public class QuestionChoice
{
    [Column("question_id")]
    public long QuestionId { get; set; }
    public Question Question { get; set; }

    [Column("choice_index")]
    public int Index { get; set; }

    [Column("choice")]
    public string Choice { get; set; }
}

[Table("question_correct_selections")]
public class QuestionCorrectSelection
{
    [Column("question_id")]
    public long QuestionId { get; set; }
    public Question Question { get; set; }

    [Column("selection")]
    public int Selection { get; set; }
}

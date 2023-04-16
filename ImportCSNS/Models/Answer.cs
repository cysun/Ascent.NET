using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

public class AnswerSheet
{
    [Column("id")]
    public long Id { get; set; }

    [Column("question_sheet_id")]
    public long QuestionSheetId { get; set; }
    public QuestionSheet QuestionSheet { get; set; }

    public List<AnswerSection> sections { get; set; }

    [Column("author_id")]
    public long UserId { get; set; }
    public User User { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }
}

public class AnswerSection
{
    [Column("id")]
    public long Id { get; set; }

    [Column("answer_sheet_id")]
    public long AnswerSheetId { get; set; }

    [Column("section_index")]
    public int Index { get; set; }

    public List<Answer> Answer { get; set; }
}

public class Answer
{
    [Column("id")]
    public long Id { get; set; }

    [Column("question_id")]
    public int QuestionId { get; set; }
    public Question Question { get; set; }

    [Column("index")]
    public int Index { get; set; }

    [Column("answer_type")]
    public string Type { get; set; }

    // Choice Answer Properties

    public List<AnswerSelection> Selections { get; set; }

    // Rating Answer Properties

    [Column("rating")]
    public int Rating { get; set; }

    // Text Answer Properties

    [Column("Text")]
    public string Text { get; set; }
}

[Table("answer_selections")]
public class AnswerSelection
{
    [Column("answer_id")]
    public long AnswerId { get; set; }
    public Answer Answer { get; set; }

    [Column("selection")]
    public int Selection { get; set; }
}

using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("answer_sheets")]
public class AnswerSheet
{
    [Column("id")]
    public long Id { get; set; }

    [Column("question_sheet_id")]
    public long QuestionSheetId { get; set; }
    public QuestionSheet QuestionSheet { get; set; }

    public List<AnswerSection> Sections { get; set; }

    [Column("author_id")]
    public long? UserId { get; set; }
    public User User { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }
}

[Table("answer_sections")]
public class AnswerSection
{
    [Column("id")]
    public long Id { get; set; }

    [Column("answer_sheet_id")]
    public long AnswerSheetId { get; set; }

    [Column("section_index")]
    public int SectionIndex { get; set; }

    public List<Answer> Answers { get; set; }
}

[Table("answers")]
public class Answer
{
    [Column("id")]
    public long Id { get; set; }

    [Column("question_id")]
    public long QuestionId { get; set; }
    public Question Question { get; set; }

    [Column("answer_section_id")]
    public long AnswerSectionId { get; set; }
    public AnswerSection AnswerSection { get; set; }

    [Column("answer_index")]
    public int AnswerIndex { get; set; }

    [Column("answer_type")]
    public string Type { get; set; }

    // Choice Answer Properties

    public List<AnswerSelection> Selections { get; set; }

    // Rating Answer Properties

    [Column("rating")]
    public int Rating { get; set; }

    // Text Answer Properties

    [Column("text")]
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

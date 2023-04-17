using System.ComponentModel.DataAnnotations.Schema;

namespace ImportCSNS.Models;

[Table("surveys")]
public class Survey
{
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string Name { get; set; }

    [Column("question_sheet_id")]
    public long QuestionSheetId { get; set; }
    public QuestionSheet questionSheet { get; set; }

    [Column("publish_date")]
    public DateTime? PublishDate { get; set; }

    [Column("close_date")]
    public DateTime? CloseDate { get; set; }

    [Column("date")]
    public DateTime Date { get; set; }

    [Column("deleted")]
    public bool IsDeleted { get; set; }

    public List<SurveyResponse> Responses { get; set; }
}

[Table("survey_responses")]
public class SurveyResponse
{
    [Column("id")]
    public long Id { get; set; }

    [Column("survey_id")]
    public long SurveyId { get; set; }
    public Survey Survey { get; set; }

    [Column("answer_sheet_id")]
    public long AnswerSheetId { get; set; }
    public AnswerSheet AnswerSheet { get; set; }
}

namespace Ascent.Models;

public class SurveyDataPoint
{
    public int Id { get; set; }

    public ConstituencyType ConstituencyType { get; set; }

    // This year should be academic year (i.e. Term.Year) instead of the year of the response.
    public int Year { get; set; }

    public int ProgramId { get; set; }
    public Program Program { get; set; }

    public int OutcomeId { get; set; }
    public ProgramOutcome Outcome { get; set; }

    public int SurveyId { get; set; }
    public Survey Survey { get; set; }

    public int AnswerId { get; set; }
    public SurveyAnswer Answer { get; set; }

    public int Value { get; set; }
}

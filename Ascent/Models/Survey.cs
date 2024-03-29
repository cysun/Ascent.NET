using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ascent.Models;

public class Survey
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime TimeCreated { get; set; } = DateTime.UtcNow;
    public DateTime? TimePublished { get; set; }
    public DateTime? TimeClosed { get; set; }

    public bool IsPublished => TimePublished.HasValue && TimePublished < DateTime.UtcNow;
    public bool IsClosed => TimeClosed.HasValue && TimeClosed < DateTime.UtcNow;

    public int QuestionCount { get; set; }
    public int ResponseCount { get; set; }

    // Whether to allow one person to submit multiple responses
    public bool AllowMultipleSubmissions { get; set; }

    public bool IsPinned { get; set; }
    public bool IsDeleted { get; set; }

    public Survey Clone()
    {
        return new Survey()
        {
            Name = $"Copy of {Name}",
            Description = Description,
            TimeCreated = DateTime.UtcNow,
            QuestionCount = QuestionCount,
            AllowMultipleSubmissions = AllowMultipleSubmissions
        };
    }
}

public enum QuestionType
{
    [Display(Name = "Choice Question")] Choice = 0,
    [Display(Name = "Rating Question")] Rating = 1,
    [Display(Name = "Text Question")] Text = 2,
    [Display(Name = "Section Block")] Section = 3 // Used to separate a survey into sections
}

// It's kind of ugly to put everything inside Question instead of creating subclasses like
// TextQuestion, ChoiceQuestion etc., but a) it the same relational schema anyway, and b)
// rendering code would actually be cleaner because we just need to check question type
// instead of doing typeof() + cast.
public class SurveyQuestion
{
    public int Id { get; set; }

    [MaxLength(10)]
    public QuestionType Type { get; set; }

    public string Description { get; set; }

    public Survey Survey { get; set; }
    public int SurveyId { get; set; }

    public int Index { get; set; }

    // Text Question

    public int TextLength { get; set; }

    // Rating Question

    public int MinRating { get; set; } = 1;
    public int MaxRating { get; set; } = 5;

    public bool IncludeNotApplicable { get; set; } // Whether to include an N/A option

    // Choice Question

    public List<string> Choices { get; set; } = new List<string>();

    public int MinSelection { get; set; } // Minimum # of choices that need to be selected
    public int MaxSelection { get; set; } // Maximum # of choices that can be selected

    public SurveyQuestion Clone(int newSurveyId)
    {
        return new SurveyQuestion()
        {
            Type = Type,
            Description = Description,
            SurveyId = newSurveyId,
            Index = Index,
            TextLength = TextLength,
            MinRating = MinRating,
            MaxRating = MaxRating,
            IncludeNotApplicable = IncludeNotApplicable,
            Choices = new List<string>(Choices),
            MinSelection = MinSelection,
            MaxSelection = MaxSelection
        };
    }
}

public class SurveyResponse
{
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public Guid Id { get; set; }

    public Survey Survey { get; set; }
    public int SurveyId { get; set; }

    public DateTime? TimeSubmitted { get; set; }

    public List<SurveyAnswer> Answers { get; set; } = new List<SurveyAnswer>();

    public bool IsDeleted { get; set; }

    public SurveyResponse() { }

    public SurveyResponse(Survey survey, List<SurveyQuestion> questions)
    {
        Survey = survey;
        SurveyId = survey.Id;
        foreach (var question in questions)
            Answers.Add(new SurveyAnswer(question));
    }
}

public class SurveyAnswer
{
    public int Id { get; set; }

    public SurveyQuestion Question { get; set; }
    public int QuestionId { get; set; }

    public SurveyResponse Response { get; set; }
    public Guid ResponseId { get; set; }

    // Text Answer

    public string Text { get; set; }

    // Rating Answer

    public int? Rating { get; set; }

    public bool NotApplicable { get; set; }

    // Choice Answer

    public string SingleSelection { get; set; }

    public List<bool> Selections { get; set; } = new List<bool>();

    public SurveyAnswer() { }

    public SurveyAnswer(SurveyQuestion question)
    {
        Question = question;
        QuestionId = question.Id;
        if (question.Type == QuestionType.Choice)
            for (int i = 0; i < question.Choices.Count; ++i) Selections.Add(false);
    }

    public string GetAnswerAsText()
    {
        if (Question.Type == QuestionType.Choice && Question.MaxSelection == 1)
            return SingleSelection;
        else if (Question.Type == QuestionType.Choice && Question.MaxSelection > 1)
        {
            var indexes = Selections.Select((selection, index) => new { selection, index })
                .Where(v => v.selection).Select(v => v.index);
            return string.Join(",", indexes);
        }
        else if (Question.Type == QuestionType.Rating)
            return !NotApplicable && Rating != null ? Rating.ToString() : "";
        else
            return Text;
    }
}

public enum ConstituencyType
{
    Student = 0,
    Faculty = 1,
    Alumni = 2,
    IAB = 3
}

public class OutcomeSurvey
{
    public int Id { get; set; }

    public ConstituencyType ConstituencyType { get; set; }

    public int SurveyId { get; set; }
    public Survey Survey { get; set; }

    public int ProgramId { get; set; }
    public Program Program { get; set; }

    // A list of questions, each mapped to a program outcome.
    public int[] QuestionIds { get; set; }

    public DateTime? DataImportTime { get; set; }
}

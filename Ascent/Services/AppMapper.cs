using Ascent.Models;
using Riok.Mapperly.Abstractions;

namespace Ascent.Services;

[Mapper]
public partial class AppMapper
{
    [UserMapping(Default = false)]
    private DateTime? MapToUniversalTime(DateTime? src) => src?.ToUniversalTime();

    [UserMapping(Default = false)]
    private DateTime? MapToLocalTime(DateTime? src) => src?.ToLocalTime();

    // AssignmentTemplate

    public partial AssignmentTemplateInputModel Map(AssignmentTemplate src);
    public partial AssignmentTemplate Map(AssignmentTemplateInputModel src);
    public partial void Map(AssignmentTemplateInputModel src, AssignmentTemplate dst);

    // Course

    public partial Course Map(CourseInputModel src);
    public partial CourseInputModel Map(Course src);
    public partial void Map(CourseInputModel src, Course dest);

    // CourseJournal

    public Term CodeToTerm(int src) => new Term(src);
    public int TermToCode(Term src) => src.Code;

    [MapProperty(nameof(CourseJournalInputModel.TermCode), nameof(CourseJournal.Term), Use = nameof(CodeToTerm))]
    public partial CourseJournal Map(CourseJournalInputModel src);

    [MapProperty(nameof(CourseJournal.Term), nameof(CourseJournalInputModel.TermCode), Use = nameof(TermToCode))]
    public partial CourseJournalInputModel Map(CourseJournal src);

    [MapProperty(nameof(CourseJournalInputModel.TermCode), nameof(CourseJournal.Term), Use = nameof(CodeToTerm))]
    public partial void Map(CourseJournalInputModel src, CourseJournal dest);

    [MapperIgnoreTarget(nameof(CourseJournal.Id))]
    public partial void Map(CourseJournal src, CourseJournal dest);

    // File and FileRevision

    public partial FileInputModel Map(Models.File src);
    public partial void Map(FileInputModel src, Models.File dest);

    [MapProperty(nameof(Models.File.Id), nameof(FileRevision.FileId))]
    public partial FileRevision MapToFileRevision(Models.File src);

    // Group

    public partial Group Map(GroupInputModel src);
    public partial GroupInputModel Map(Group src);
    public partial void Map(GroupInputModel src, Group dest);

    // Message

    public partial Message Map(MessageInputModel src);

    // MftIndicator

    public partial MftIndicator Map(MftIndicatorInputModel src);
    public partial void Map(MftIndicatorInputModel src, MftIndicator dest);

    // MftDistribution

    [MapperIgnoreTarget(nameof(MftDistribution.Ranks))]
    private partial MftDistribution MapWithoutRanks(MftDistributionInputModel src);

    [UserMapping(Default = true)]
    public MftDistribution Map(MftDistributionInputModel src)
    {
        var dest = MapWithoutRanks(src);

        dest.Ranks = new List<(int, int)>();
        if (!string.IsNullOrEmpty(src.Ranks))
        {
            var lines = src.Ranks.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (src.TypeAlias)
                {
                    case "AI2":
                        dest.Ranks.Add(ValueTuple.Create(int.Parse(tokens[0]), int.Parse(tokens[2])));
                        break;
                    case "AI3":
                        dest.Ranks.Add(ValueTuple.Create(int.Parse(tokens[0]), int.Parse(tokens[3])));
                        break;
                    default:
                        dest.Ranks.Add(ValueTuple.Create(int.Parse(tokens[0]), int.Parse(tokens[1])));
                        break;
                }
            }
        }

        return dest;
    }

    // OutcomeSurvey

    public partial OutcomeSurvey Map(OutcomeSurveyInputModel src);

    // Page and PageRevision

    public partial Page Map(PageInputModel src);

    [MapProperty(nameof(Page.Id), nameof(PageRevision.PageId))]
    [MapperIgnoreTarget(nameof(PageRevision.TimeCreated))]
    public partial PageRevision MapToPageRevision(Page src);

    // Person

    public partial Person Map(PersonInputModel src);
    public partial PersonInputModel Map(Person src);
    public partial void Map(PersonInputModel src, Person dest);

    // Project

    public partial Project Map(ProjectInputModel src);
    public partial ProjectInputModel Map(Project src);
    public partial void Map(ProjectInputModel src, Project dest);

    // ProjectResource

    public partial ProjectResource Map(ProjectResourceInputModel src);
    public partial ProjectResourceInputModel Map(ProjectResource src);
    public partial void Map(ProjectResourceInputModel src, ProjectResource dest);

    // Program

    [MapperIgnoreTarget(nameof(Models.Program.Outcomes))]
    private partial Models.Program MapWithoutOutcomes(ProgramInputModel src);

    [UserMapping(Default = true)]
    public Models.Program Map(ProgramInputModel src)
    {
        var program = MapWithoutOutcomes(src);

        program.Outcomes = new List<ProgramOutcome>();
        for (var i = 0; i < src.Outcomes.Count; ++i)
        {
            program.Outcomes.Add(new ProgramOutcome
            {
                Index = i,
                Text = src.Outcomes[i],
                Description = new Page { Subject = $"{src.Name} Outcome {i + 1} Description" }
            });
        }

        return program;
    }

    private string Map(ProgramOutcome src) => src.Text;
    public partial ProgramInputModel Map(Models.Program src);

    [MapperIgnoreSource(nameof(Models.Program.Outcomes))]
    private partial void MapWithoutOutcomes(ProgramInputModel src, Models.Program dest);

    [UserMapping(Default = true)]
    public void Map(ProgramInputModel src, Models.Program dest)
    {
        MapWithoutOutcomes(src, dest);

        if (dest.Outcomes.Count == src.Outcomes?.Count) // # of outcomes cannot be changed
        {
            for (int i = 0; i < dest.Outcomes.Count; ++i)
                dest.Outcomes[i].Text = src.Outcomes[i];
        }
    }

    // ProgramModule

    public partial ProgramModule Map(ProgramModuleInputModel src);
    public partial ProgramModuleInputModel Map(ProgramModule src);
    public partial void Map(ProgramModuleInputModel src, ProgramModule dest);

    // Rubric

    [MapProperty(nameof(RubricInputModel.TimePublished), nameof(Rubric.TimePublished),
        Use = nameof(MapToUniversalTime))]
    public partial Rubric Map(RubricInputModel src);

    [MapProperty(nameof(Rubric.TimePublished), nameof(RubricInputModel.TimePublished), Use = nameof(MapToLocalTime))]
    public partial RubricInputModel Map(Rubric src);

    [MapperIgnoreTarget(nameof(Rubric.TimePublished))]
    private partial void MapWithoutTimePublished(RubricInputModel src, Rubric dest);

    [UserMapping(Default = true)]
    public void Map(RubricInputModel src, Rubric dest)
    {
        MapWithoutTimePublished(src, dest);

        if (!dest.IsPublished) // skip if already published
            dest.TimePublished = src.TimePublished?.ToUniversalTime();
    }

    // RubricCriterion

    public partial RubricCriterionInputModel Map(RubricCriterion src);
    public partial void Map(RubricCriterionInputModel src, RubricCriterion dest);

    // Survey

    [MapProperty(nameof(SurveyInputModel.TimePublished), nameof(Survey.TimePublished),
        Use = nameof(MapToUniversalTime))]
    [MapProperty(nameof(SurveyInputModel.TimeClosed), nameof(Survey.TimeClosed), Use = nameof(MapToUniversalTime))]
    public partial Survey Map(SurveyInputModel src);

    [MapProperty(nameof(Survey.TimePublished), nameof(SurveyInputModel.TimePublished), Use = nameof(MapToLocalTime))]
    [MapProperty(nameof(Survey.TimeClosed), nameof(Survey.TimeClosed), Use = nameof(MapToLocalTime))]
    public partial SurveyInputModel Map(Survey src);

    [MapperIgnoreTarget(nameof(Survey.TimePublished))]
    [MapProperty(nameof(SurveyInputModel.TimeClosed), nameof(Survey.TimeClosed), Use = nameof(MapToUniversalTime))]
    private partial void MapWithoutTimePublished(SurveyInputModel src, Survey dest);

    [UserMapping(Default = true)]
    public void Map(SurveyInputModel src, Survey dest)
    {
        MapWithoutTimePublished(src, dest);

        if (!dest.IsPublished) // skip if already published
            dest.TimePublished = src.TimePublished?.ToUniversalTime();
    }

    // SurveyQuestion

    public partial SurveyQuestion Map(SurveyQuestionInputModel src);
    public partial SurveyQuestionInputModel Map(SurveyQuestion src);
    public partial void Map(SurveyQuestionInputModel src, SurveyQuestion dest);
}

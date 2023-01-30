using System.Text.RegularExpressions;
using Ascent.Models;
using AutoMapper;

namespace Ascent.Services;

public class MapperProfile : Profile
{
    public MapperProfile()
    {
        CreateMap<Term, string>().ConvertUsing(source => source != null ? source.ShortName : null);
        CreateMap<string, Term>().ConvertUsing(new StringToTermConverter());

        CreateMap<PersonInputModel, Person>();
        CreateMap<Person, PersonInputModel>();
        CreateMap<GroupInputModel, Models.Group>();
        CreateMap<Models.Group, GroupInputModel>();
        CreateMap<CourseInputModel, Course>();
        CreateMap<Course, CourseInputModel>();
        CreateMap<PageInputModel, Page>();
        CreateMap<Page, PageInputModel>();

        CreateMap<Models.File, FileInputModel>();
        CreateMap<FileInputModel, Models.File>();
        CreateMap<Models.File, FileRevision>().ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Page, PageRevision>().ForMember(dest => dest.PageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TimeCreated, opt => opt.Ignore());

        CreateMap<MessageInputModel, Message>();

        CreateMap<string, List<(int, int)>>().ConvertUsing(new StringToRanksConverter());
        CreateMap<MftDistributionInputModel, MftDistribution>();
        CreateMap<MftIndicatorInputModel, MftIndicator>();

        CreateMap<SurveyInputModel, Survey>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) =>
                dest.IsPublished ? dest.TimePublished : src.TimePublished?.ToUniversalTime())) // skip if already published
            .ForMember(dest => dest.TimeClosed, opt => opt.MapFrom((src, dest) => src.TimeClosed?.ToUniversalTime()));
        CreateMap<Survey, SurveyInputModel>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) => src.TimePublished?.ToLocalTime()))
            .ForMember(dest => dest.TimeClosed, opt => opt.MapFrom((src, dest) => src.TimeClosed?.ToLocalTime()));

        CreateMap<SurveyQuestionInputModel, SurveyQuestion>();
        CreateMap<SurveyQuestion, SurveyQuestionInputModel>();

        CreateMap<ProgramOutcome, string>().ConvertUsing(new OutcomeToStringConverter());
        CreateMap<ProgramInputModel, Models.Program>()
            .ForMember(dest => dest.Outcomes, opt => opt.Ignore()); // Handled in controller
        CreateMap<Models.Program, ProgramInputModel>();

        CreateMap<ProgramModuleInputModel, ProgramModule>();
        CreateMap<ProgramModule, ProgramModuleInputModel>();

        CreateMap<RubricInputModel, Rubric>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) =>
                dest.IsPublished ? dest.TimePublished : src.TimePublished?.ToUniversalTime())); // skip if already published
        CreateMap<Rubric, RubricInputModel>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) => src.TimePublished?.ToLocalTime()));

        CreateMap<RubricCriterionInputModel, RubricCriterion>();
        CreateMap<RubricCriterion, RubricCriterionInputModel>();

        CreateMap<RubricRatingInputModel, RubricRating>();
        CreateMap<RubricRating, RubricRatingInputModel>();

        CreateMap<ProjectInputModel, Project>();
        CreateMap<Project, ProjectInputModel>();

        CreateMap<ProjectResourceInputModel, ProjectResource>();
        CreateMap<ProjectResource, ProjectResourceInputModel>();
    }
}

public class StringToTermConverter : ITypeConverter<string, Term>
{
    public Term Convert(string source, Term destination, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source)) return null;

        var pattern = new Regex(@"^[fFwWsWxX]\d{2}$");
        return pattern.IsMatch(source) ? new Term(source) : destination;
    }
}

public class StringToRanksConverter : ITypeConverter<string, List<(int, int)>>
{
    public List<(int, int)> Convert(string source, List<(int, int)> destination, ResolutionContext context)
    {
        destination = new List<(int, int)>();

        if (!string.IsNullOrEmpty(source))
        {
            var lines = source.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                switch (context.Items["type"])
                {
                    case "AI2":
                        destination.Add(ValueTuple.Create(int.Parse(tokens[0]), int.Parse(tokens[2])));
                        break;
                    case "AI3":
                        destination.Add(ValueTuple.Create(int.Parse(tokens[0]), int.Parse(tokens[3])));
                        break;
                    default:
                        destination.Add(ValueTuple.Create(int.Parse(tokens[0]), int.Parse(tokens[1])));
                        break;
                }
            }
        }

        return destination;
    }
}

public class OutcomeToStringConverter : ITypeConverter<ProgramOutcome, string>
{
    public string Convert(ProgramOutcome source, string destination, ResolutionContext context)
    {
        return source.Text;
    }
}

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
        CreateMap<CourseInputModel, Course>();
        CreateMap<Course, CourseInputModel>();
        CreateMap<PageInputModel, Page>();
        CreateMap<Page, PageInputModel>();

        CreateMap<Models.File, FileInputModel>();
        CreateMap<FileInputModel, Models.File>();
        CreateMap<Models.File, FileRevision>().ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.Id));

        CreateMap<Page, PageRevision>().ForMember(dest => dest.PageId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.TimeCreated, opt => opt.Ignore());

        CreateMap<string, List<(int, int)>>().ConvertUsing(new StringToRanksConverter());
        CreateMap<MftDistributionInputModel, MftDistribution>();
        CreateMap<MftIndicatorInputModel, MftIndicator>();

        CreateMap<SurveyInputModel, Survey>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) => src.TimePublished?.ToUniversalTime()))
            .ForMember(dest => dest.TimeClosed, opt => opt.MapFrom((src, dest) => src.TimeClosed?.ToUniversalTime()));
        CreateMap<Survey, SurveyInputModel>()
            .ForMember(dest => dest.TimePublished, opt => opt.MapFrom((src, dest) => src.TimePublished?.ToLocalTime()))
            .ForMember(dest => dest.TimeClosed, opt => opt.MapFrom((src, dest) => src.TimeClosed?.ToLocalTime()));

        CreateMap<SurveyQuestionInputModel, SurveyQuestion>();
        CreateMap<SurveyQuestion, SurveyQuestionInputModel>();
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

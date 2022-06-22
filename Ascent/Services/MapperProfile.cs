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

        CreateMap<Models.File, FileHistory>().ForMember(dest => dest.FileId, opt => opt.MapFrom(src => src.Id));
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

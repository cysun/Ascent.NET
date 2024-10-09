using System.Security.Claims;
using System.Text.RegularExpressions;
using Ascent.Models;

namespace Ascent.Helpers;

public static class Utils
{
    // We assume names are either in "FirstName LastName" format or "LastName, FirstName" format.
    // If a name has multiple words like "name1 name2 name3", the first/last one is consider first/last name
    // (the rest would be middle name).
    public static (string FirstName, string LastName) SplitName(string name)
    {
        if (string.IsNullOrEmpty(name)) return ("", "");

        var index = name.IndexOf(',');
        if (index >= 0)
            return (name.Substring(index + 1), name.Substring(0, index));
        else
            return (name.Substring(0, name.IndexOf(" ")), name.Substring(name.LastIndexOf(" ") + 1));
    }

    // academicYear must be in the form of "<year>-<year+1>", e.g. "2020-2021"
    public static (DateTime? StartTime, DateTime? EndTime) AcademicYear(string academicYear)
    {
        if (string.IsNullOrEmpty(academicYear)) return (null, null);

        var years = academicYear.Split('-');
        if (years.Length != 2) return (null, null);

        int startYear, endYear;
        var success1 = int.TryParse(years[0], out startYear);
        var success2 = int.TryParse(years[1], out endYear);
        if (!success1 || !success2 || endYear - startYear != 1)
            return (null, null);

        return ((new Term(startYear, "FALL")).StartTime, (new Term(endYear, "FALL")).StartTime);
    }

    // From https://stackoverflow.com/questions/18153998/how-do-i-remove-all-html-tags-from-a-string-without-knowing-which-tags-are-in-it
    public static string StripHtmlTags(string input) => input != null ? Regex.Replace(input, "<.*?>", String.Empty) : null;

    public static List<Term> GetTerms(int n = 6)
    {
        var terms = new List<Term>();
        var term = new Term();
        for (int i = 0; i < n; i++)
        {
            terms.Add(term);
            term = term.Previous();
        }
        return terms;
    }

    // Get the "name" claim value from Identity, and if "name" claim does not exist, get NameIdentifier claim value.
    public static string GetName(this ClaimsPrincipal user)
    {
        var name = user.FindFirstValue("name");
        if (string.IsNullOrEmpty(name))
            name = user.FindFirstValue(ClaimTypes.NameIdentifier);
        return name;
    }
}

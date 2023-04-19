using Ascent.Models;

namespace Ascent.Helpers;

public class Utils
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
}

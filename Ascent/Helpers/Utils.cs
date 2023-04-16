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
}

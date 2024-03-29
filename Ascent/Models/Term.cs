using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models;

/// <summary>
/// Term is represented by a "code" for easy handling. The formula to calculate
/// the code is (year-1900)*10 + [1, 3, 6, 9] for Winter, Spring, Summer, and
/// Fall, respectively. For example, the code for Spring 2022 is 1223, while
/// the code 1199 represents Fall 2019.
/// </summary>
[Owned]
public class Term
{
    public int Code { get; set; }

    public string Name
    {
        get
        {
            string s;
            switch (Code % 10)
            {
                case 1:
                    s = "Winter";
                    break;
                case 3:
                    s = "Spring";
                    break;
                case 6:
                    s = "Summer";
                    break;
                case 9:
                    s = "Fall";
                    break;
                default:
                    s = "UNKNOWN";
                    break;
            }
            return s + " " + (Code / 10 + 1900);
        }
    }

    public string ShortName
    {
        get
        {
            string s;
            switch (Code % 10)
            {
                case 1:
                    s = "W";
                    break;
                case 3:
                    s = "S";
                    break;
                case 6:
                    s = "X";
                    break;
                case 9:
                    s = "F";
                    break;
                default:
                    s = "U";
                    break;
            }
            int year = ((Code / 10) + 1900) % 100;
            return s + (year < 10 ? "0" + year : year);
        }
    }

    // Academic Year is like 2021-2022. An academic year starts in fall and ends in summer.
    public string AcademicYear
    {
        get
        {
            int year = Code / 10 + 1900;
            return Code % 10 == 9 ? $"{year}-{year + 1}" : $"{year - 1}-{year}";
        }
    }

    // A shorthand for academic year showing only the starting year. For example, 2021 means 2021-2022.
    public int Year
    {
        get
        {
            int year = Code / 10 + 1900;
            return Code % 10 == 9 ? year : year - 1;
        }
    }

    public DateTime StartTime
    {
        get
        {
            int year = Code / 10 + 1900;

            // https://stackoverflow.com/questions/5664862/whats-the-simplest-way-to-calculate-the-monday-in-the-first-week-of-the-year
            Calendar calendar = new CultureInfo("en-US").Calendar;
            var firstDay = new DateTime(year, 1, 1, calendar);
            var winterStartTime = (new DateTime(year, 1, (8 - (int)firstDay.DayOfWeek) % 7 + 1, calendar)).ToUniversalTime();

            switch (Code % 10)
            {
                case 1: // Winter term: week 1-3
                    return winterStartTime;
                case 3: // Spring term: week 4-21
                    return winterStartTime.AddDays(3 * 7);
                case 6: // Summer term: week 22-33
                    return winterStartTime.AddDays(21 * 7);
                default:// Fall term: week 34-
                    // We'll count the week befor fall semester as part of fall semester because the stuff
                    // happened in that week should be consider things for the new academic year.
                    return winterStartTime.AddDays(32 * 7);
            }
        }
    }

    public Term()
    {
        SetCode(DateTime.Now);
    }

    public Term(int code)
    {
        Code = code;
    }

    public Term(DateTime date)
    {
        SetCode(date);
    }

    public Term(int year, string season)
    {
        SetCode(year, season);
    }

    public Term(string s)
    {
        string[] tokens = s.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length < 2) // short term string like S22
        {
            int year = 2000 + int.Parse(s.Substring(1, 2));
            SetCode(year, s.Substring(0, 1));
        }
        else // long term string Spring 2022
        {
            SetCode(int.Parse(tokens[1]), tokens[0]);
        }
    }

    public void SetCode(DateTime date)
    {
        Code = (date.Year - 1900) * 10;

        Calendar calendar = new CultureInfo("en-US").Calendar;
        int week = calendar.GetWeekOfYear(date, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
        if (week < 4)
            Code += 1; // Winter term: week 1-3
        else if (week < 22)
            Code += 3; // Spring term: week 4-21
        else if (week < 34)
            Code += 6; // Summer term: week 22-33
        else
            Code += 9; // Fall term: week 34-
    }

    public void SetCode(int year, string season)
    {
        Code = (year - 1900) * 10;

        season = season.ToUpper();
        switch (season.ToUpper())
        {
            case "WINTER":
            case "W":
                Code += 1;
                break;

            case "SPRING":
            case "S":
                Code += 3;
                break;

            case "SUMMER":
            case "X":
                Code += 6;
                break;

            case "FALL":
            case "F":
            default:
                Code += 9;
                break;
        }
    }

    public Term Previous()
    {
        int yearCode = Code / 10;
        int termSuffix = Code % 10;

        switch (termSuffix)
        {
            case 9:
                termSuffix = 6;
                break;
            case 6:
                termSuffix = 3;
                break;
            case 3:
                termSuffix = 1;
                break;
            default:
                termSuffix = 9;
                --yearCode;
                break;
        }

        return new Term(yearCode * 10 + termSuffix);
    }

    public Term Next()
    {
        int yearCode = Code / 10;
        int termSuffix = Code % 10;

        switch (termSuffix)
        {
            case 1:
                termSuffix = 3;
                break;
            case 3:
                termSuffix = 6;
                break;
            case 6:
                termSuffix = 9;
                break;
            default:
                termSuffix = 1;
                ++yearCode;
                break;
        }

        return new Term(yearCode * 10 + termSuffix);
    }
}

using System.Globalization;
using Microsoft.EntityFrameworkCore;

namespace Ascent.Models
{
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

        public Term()
        {
            SetCode(DateTime.Now);
        }

        public Term(int code)
        {
            Code = code;
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
    }
}

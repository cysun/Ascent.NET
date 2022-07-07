using System.ComponentModel.DataAnnotations;

namespace Ascent.Models
{
    public class MftScore
    {
        public int Id { get; set; }

        public DateOnly Date { get; set; }

        [Required, MaxLength(100)]
        public string StudentId { get; set; }

        [Required, MaxLength(255)]
        public string FirstName { get; set; }

        [Required, MaxLength(255)]
        public string LastName { get; set; }

        public int Score { get; set; }

        public int? Percentile { get; set; }
    }

    public class MftIndicator
    {
        public int Id { get; set; }

        public DateOnly Date { get; set; }

        public int NumOfStudents { get; set; }

        public int[] Scores { get; set; } = new int[3];

        public int?[] Percentiles { get; set; } = new int?[3];
    }

    public class MftDistributionType
    {
        public int Id { get; set; }

        [MaxLength(15)]
        public string Alias { get; set; }

        [MaxLength(255)]
        public string Name { get; set; }

        public int Min { get; set; }
        public int Max { get; set; }

        [MaxLength(32)]
        public string ValueLabel { get; set; }
    }

    public class MftDistribution
    {
        public int Id { get; set; }

        public int Year { get; set; }

        public string TypeAlias { get; set; }
        public MftDistributionType Type { get; set; }

        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }

        public int NumOfSamples { get; set; }

        public double Mean { get; set; }
        public double Median { get; set; }
        public double StdDev { get; set; }

        public List<(int, int)> Ranks { get; set; } = new List<(int, int)>();
    }
}

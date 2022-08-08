using System.ComponentModel.DataAnnotations;

namespace Ascent.Models
{
    public class Program
    {
        public int Id { get; set; }

        [Required, MaxLength(255)]
        public string Name { get; set; }

        public bool HasObjectives { get; set; } // Non-ABET programs may not have PEOs
        public List<string> Objectives { get; set; }

        public int? ObjectivesDescriptionId { get; set; }
        public Page ObjectivesDescription { get; set; }

        public List<ProgramOutcome> Outcomes { get; set; }

        public List<ProgramModule> Modules { get; set; }

        public bool IsDeleted { get; set; }
    }

    public class ProgramOutcome
    {
        public int Id { get; set; }

        public int ProgramId { get; set; }
        public Program Program { get; set; }

        public int Index { get; set; }

        [Required]
        public string Text { get; set; }

        public int DescriptionId { get; set; }
        public Page Description { get; set; }
    }

    public class ProgramModule
    {
        public int Id { get; set; }

        public int ProgramId { get; set; }
        public Program Program { get; set; }

        public int Index { get; set; }

        [Required, MaxLength(64)]
        public string Name { get; set; }

        public List<ProgramItem> Items { get; set; }
    }

    public enum ItemType { File, Page }

    public class ProgramItem
    {
        public int Id { get; set; }

        public int ModuleId { get; set; }
        public ProgramModule Module { get; set; }

        public int Index { get; set; }

        [MaxLength(10)]
        public ItemType Type { get; set; }

        public int? FileId { get; set; }
        public File File { get; set; }

        public int? PageId { get; set; }
        public Page Page { get; set; }
    }
}

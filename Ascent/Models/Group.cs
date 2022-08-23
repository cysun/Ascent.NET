using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public enum EmailPreference { School, Personal }

public class Group
{
    public int Id { get; set; }

    [Required, MaxLength(32)]
    public string Name { get; set; }

    public string Description { get; set; }

    [MaxLength(10)]
    public EmailPreference EmailPreference { get; set; }

    public int MemberCount { get; set; }

    // The system recognizes a few "virtual" groups, e.g. BS Alumni and MS Alumni.
    // Virtual groups don't have explicitly specified members; instead, their members are
    // based on certain query criteria. For all the recognized virtual groups, please see
    // GroupService.GetMembers().
    public bool IsVirtual { get; set; }
}

public class GroupMember
{
    public int GroupId { get; set; }
    public Group Group { get; set; }

    public int PersonId { get; set; }
    public Person Person { get; set; }
}

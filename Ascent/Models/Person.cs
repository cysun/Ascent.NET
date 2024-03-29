using System.ComponentModel.DataAnnotations;

namespace Ascent.Models;

public class Person
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string CampusId { get; set; }

    [Required, MaxLength(255)]
    public string FirstName { get; set; }

    [MaxLength(255)]
    public string MiddleName { get; set; }

    [Required, MaxLength(255)]
    public string LastName { get; set; }

    [MaxLength(100)]
    public string ScreenName { get; set; }

    [MaxLength(255)]
    public string SchoolEmail { get; set; }

    [MaxLength(255)]
    public string PersonalEmail { get; set; }

    // BS Graduation Term
    public Term BgTerm { get; set; }

    // MS Grduation Term
    public Term MgTerm { get; set; }

    // User ID on Canvas
    public int? CanvasId { get; set; }

    public bool IsDeleted { get; set; }

    public string FullName => $"{FirstName} {LastName}";
    public string FullName2 => $"{LastName}, {FirstName}";

    public string Email => SchoolEmail ?? PersonalEmail;

    public string GetPreferredEmail(EmailPreference preference) => preference == EmailPreference.Personal ?
        PersonalEmail ?? SchoolEmail : SchoolEmail ?? PersonalEmail;

    public void UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return;

        if (email.ToLower().Contains("calstatela"))
            SchoolEmail ??= email;
        else
            PersonalEmail ??= email;
    }
}

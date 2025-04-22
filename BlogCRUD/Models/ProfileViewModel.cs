using System.ComponentModel.DataAnnotations;

public class ProfileViewModel
{
    [Required]
    [Display(Name = "Complete Name")]
    public string FullName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public List<string> Roles { get; set; } = new();
}

using System.ComponentModel.DataAnnotations;

namespace IdentityService.Pages.Register;

public class RegisterViewModel
{
    [Required] public string Username { get; set; }
    [Required] public string Email { get; set; }
    [Required] public string FullName { get; set; }
    [Required] public string Password { get; set; }
    public string? ReturnUrl { get; set; }
    public string? Button { get; set; }
}
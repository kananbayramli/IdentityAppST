using System.ComponentModel.DataAnnotations;

namespace IdentityApp.ViewModels;

public class ResetPasswordModel
{

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Token { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage ="Parol duzgun gelmir...")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
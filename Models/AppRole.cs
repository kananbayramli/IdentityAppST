using Microsoft.AspNetCore.Identity;

namespace IdentityApp.Models;

public class AppRole : IdentityRole
{
    public string FullName { get; set; } = string.Empty;
}
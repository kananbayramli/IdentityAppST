using Microsoft.AspNetCore.Mvc;
using IdentityApp.Models;
using Microsoft.AspNetCore.Identity;
using IdentityApp.ViewModels;


namespace IdentityApp.Controllers;

public class AccountController : Controller
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    

    public AccountController(
        RoleManager<AppRole> roleManager, 
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
    }


    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if(ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if(user != null)
            {
                await _signInManager.SignOutAsync();

                var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, true);

                if(result.Succeeded)
                {
                    await _userManager.ResetAccessFailedCountAsync(user);
                    await _userManager.SetLockoutEndDateAsync(user, null);

                    return RedirectToAction("Index", "Home");
                }
                else if(result.IsLockedOut)
                {
                    var lockOutDate = await _userManager.GetLockoutEndDateAsync(user);
                    var timeLeft = lockOutDate.Value - DateTime.UtcNow;
                    ModelState.AddModelError("", $"Hesabiniz bloklandi, Zehmet olmasa {timeLeft.Minutes} deqiqe sonra yeniden yoxlayin!");
                }
                else{ModelState.AddModelError("", "Xetali email ve ya parol...");}
            }
            else
            {
                ModelState.AddModelError("", "Xetali email ve ya parol...");
            }
        }


        return View(model);
    }
}

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
    private IEmailSender _emailSender;
    

    public AccountController(
        RoleManager<AppRole> roleManager, 
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IEmailSender emailSender)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _signInManager = signInManager;
        _emailSender = emailSender;
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

                if(!await _userManager.IsEmailConfirmedAsync(user))
                {
                    ModelState.AddModelError("", "Hesabinizi tesdiq edin !");
                    return View(model);
                }

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


    public IActionResult Create()
    {
        return View();
    }


    [HttpPost]
    public async  Task<IActionResult> Create(CreateViewModel model)
    {
        if(ModelState.IsValid)
        {
            var user = new AppUser{UserName = model.UserName, Email = model.Email, FullName= model.FullName};
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if(result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var url = Url.Action("ConfirmEmail","Account", new {user.Id, token});

                // email
                await _emailSender.SendEmailAsync(user.Email, "Hesab tesdiqi", $"Zehmet olmasa hesabin tesdiqi ucun linke <a href='https://localhost:5213{url}'>klikleyin</a>");

                TempData["message"] = "Email hesabinizdaki tesdiq mailini klikleyin";
                return RedirectToAction("Login", "Account");
            }

            foreach(IdentityError err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
        }
        return View(model);
    }


    public async Task<IActionResult> ConfirmEmail(string Id, string token)
    {
        if(Id == null || token == null)
        {
            TempData["message"] = "Uygun olmayan token";
            return View();
        }

        var user = await _userManager.FindByIdAsync(Id);
        
        if(user != null)
        {
            var result = await _userManager.ConfirmEmailAsync(user, token);

            if(result.Succeeded)
            {
                TempData["message"] = "Hesabiniz tesdiq edilmishdir!";
                return RedirectToAction("Login", "Account");
            }
        }

        TempData["message"] = "Bele bir user tapilmadi!";
        return View();
    }


    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login");

    }

    public async Task<IActionResult> ForgotPassword(string Email)
    {
        if(string.IsNullOrEmpty(Email))
        {
            TempData["message"] = "Email adresini daxil edin!";
            return View();
        }

        var user = await _userManager.FindByEmailAsync(Email);
        if(user == null)
        {
            TempData["message"] = "Email adrese uygun hesab tapilmadi!";
            return View();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = Url.Action("ResetPassword", "Account", new {user.Id, token});

        await _emailSender.SendEmailAsync(Email, "Parol Deyishme", $"Parolunuzu deyishmek ucun linke <a href='http://localhost:5213{url}'>klikleyin</a>.");

        TempData["message"] = "Emailinize gonderilen linkle shifrenizi deyishe bilersiz";

        return View();
    }


    public IActionResult ResetPassword(string Id, string token)
    {
        if(Id == null || token == null)
        {
            return RedirectToAction("Login");
        }
        var model = new ResetPasswordModel{Token = token};
        return View(model);
    }


    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        if(ModelState.IsValid)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if(user == null)
            {
                TempData["message"] = "Bu mailli istifadeci yoxdur!";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if(result.Succeeded)
            {
                TempData["message"] = "Shifreniz deyishdirildi";
                return RedirectToAction("Login");
            }

            foreach (IdentityError err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
        }
        return View(model);
    }

}

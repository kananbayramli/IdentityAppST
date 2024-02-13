using IdentityApp.Models;
using IdentityApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace IdentityApp.Controllers;

public class UsersController : Controller
{
    private UserManager<AppUser> _userManager;
    public UsersController(UserManager<AppUser> userManager)
    {
        _userManager = userManager;
    }

    public IActionResult Index()
    {
        return View(_userManager.Users);
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
            var user = new AppUser{UserName = model.Email, Email = model.Email, FullName= model.FullName};
            IdentityResult result = await _userManager.CreateAsync(user, model.Password);

            if(result.Succeeded)
            {
                return RedirectToAction("Index");
            }

            foreach(IdentityError err in result.Errors)
            {
                ModelState.AddModelError("", err.Description);
            }
        }
        return View(model);
    }




    public async Task<IActionResult> Edit(string id)
    {
        if(id == null)
        {
            return RedirectToAction("Index");
        }

        var user =await _userManager.FindByIdAsync(id);

        if(user != null)
        {
            return View(new EditViewModel{
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email
            });
        }

        return RedirectToAction("Index");    
    }


    [HttpPost]
    public async Task<IActionResult> Edit(string id, EditViewModel model)
    {
        if(id != model.Id)
        {
            return NotFound();
        }


        if(ModelState.IsValid)
        {
            var user = await _userManager.FindByIdAsync(model.Id);

            if(user != null)
            {
                user.Email = model.Email;
                user.FullName = model.FullName;

                var result = await _userManager.UpdateAsync(user);

                if(result.Succeeded && !string.IsNullOrEmpty(model.Password))
                {
                    //burada userin parolunu silirik
                    await _userManager.RemovePasswordAsync(user);
                    //burada yeni parol qoyurug
                    await _userManager.AddPasswordAsync(user, model.Password);
                }


                if(result.Succeeded){return RedirectToAction("Index");}


                foreach(IdentityError err in result.Errors)
                {
                    ModelState.AddModelError("", err.Description);
                }
            }
        }

        return View(model);
    }
}
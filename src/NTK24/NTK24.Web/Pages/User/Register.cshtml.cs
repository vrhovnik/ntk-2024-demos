﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NTK24.Interfaces;
using NTK24.Models;
using NTK24.Web.Base;

namespace NTK24.Web.Pages.User;

[AllowAnonymous]
public class RegisterPageModel(ILogger<RegisterPageModel> logger, IUserService userService)
    : BasePageModel
{
    public void OnGetAsync() => logger.LogInformation("Loaded register page at {DateLoaded}", DateTime.Now);

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrEmpty(NewUser.Email) || string.IsNullOrEmpty(NewUser.Password))
        {
            Message = "Enter required data: Email and Password";
            logger.LogError("Data is not entered, missing either Email or Password.");
            return Page();
        }

        //check if email is already on
        var user = await userService.FindAsync(NewUser.Email);
        if (user != null)
        {
            Message = $"User with email {user.Email} already exists in database, try new one";
            logger.LogWarning(Message);
            return Page();
        }

        var ttaUser = await userService.InsertAsync(NewUser);
        
        await HttpContext.SignInAsync(ttaUser.GenerateClaims());

        return RedirectToPage("/User/Dashboard");
    }

    [BindProperty] public SulUser NewUser { get; set; }
}
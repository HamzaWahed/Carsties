using System.Security.Claims;
using Duende.IdentityModel;
using IdentityService.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace IdentityService.Pages.Register;

[SecurityHeaders]
[AllowAnonymous]
public class Index(UserManager<ApplicationUser> userManager) : PageModel
{
    [BindProperty] public RegisterViewModel Input { get; set; }
    [BindProperty] public bool RegisterSuccess { get; set; }

    public IActionResult OnGet(string returnUrl)
    {
        Input = new RegisterViewModel
        {
            ReturnUrl = returnUrl
        };

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (Input.Button != "register") return Redirect("~/");

        if (ModelState.IsValid)
        {
            var user = new ApplicationUser()
            {
                UserName = Input.Username,
                Email = Input.Email,
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    // Log the error to the console or your logging framework
                    Console.WriteLine($"Identity Error: {error.Code} - {error.Description}");

                    // Also add it to the ModelState to display it on the page
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                // Return the page so the user can see the validation errors
                return Page();
            }

            await userManager.AddClaimsAsync(user, new Claim[]
            {
                new(JwtClaimTypes.Name, Input.FullName)
            });
        }

        return Page();
    }
}
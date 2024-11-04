using FinancialChatRoomApp.FinancialChatRoom.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace FinancialChatRoomApp.Pages.Account;
public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;

    public LoginModel(SignInManager<User> signInManager)
    {
        _signInManager = signInManager;
    }

    [BindProperty]
    public LoginInputModel Input { get; set; }

    public class LoginInputModel
    {
        [Required]
        public string Username { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(Input.Username, Input.Password, false, false);
            if (result.Succeeded)
            {
                return RedirectToPage("/Chatroom");
            }
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        }
        return Page();
    }
}
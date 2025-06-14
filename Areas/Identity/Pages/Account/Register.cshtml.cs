using FileShare.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using FileShare.Resources.Areas.Identity.Pages.Account;

namespace FileShare.Areas.Identity.Pages.Account;

public class RegisterModel : PageModel
{
    private readonly SignInManager<UserModel> _signInManager;
    private readonly IStringLocalizer<RegisterModel> _viewLocalizer;
    private readonly UserManager<UserModel> _userManager;
    private readonly IUserStore<UserModel> _userStore;
    private readonly IUserEmailStore<UserModel> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly IEmailSender _emailSender;

    public RegisterModel(
        IStringLocalizer<RegisterModel> viewLocalizer,
        UserManager<UserModel> userManager,
        IUserStore<UserModel> userStore,
        SignInManager<UserModel> signInManager,
        ILogger<RegisterModel> logger,
        IEmailSender emailSender)
    {
        _viewLocalizer = viewLocalizer;
        _userManager = userManager;
        _userStore = userStore;
        _emailStore = GetEmailStore();
        _signInManager = signInManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }

    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public class InputModel
    {
        [Required(ErrorMessageResourceName = "EmailFieldRequired", ErrorMessageResourceType = typeof(Register))]
        [EmailAddress(ErrorMessageResourceName = "EmailValid", ErrorMessageResourceType = typeof(Register))]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessageResourceName = "PasswordFieldRequired", ErrorMessageResourceType = typeof(Register))]
        [StringLength(100, ErrorMessageResourceName = "PasswordLength", ErrorMessageResourceType = typeof(Register), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Пароль")]
        public string Password { get; set; }

        [Required(ErrorMessageResourceName = "PasswordConfirmRequired", ErrorMessageResourceType = typeof(Register))]
        [DataType(DataType.Password)]
        [Display(Name = "Подтверждение пароля")]
        [Compare("Password", ErrorMessageResourceName = "PasswordCompare", ErrorMessageResourceType = typeof(Register))]
        public string ConfirmPassword { get; set; }
    }


    public async Task OnGetAsync(string returnUrl = null)
    {
        ReturnUrl = returnUrl;
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        if (ModelState.IsValid)
        {
            var user = CreateUser();

            await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);
            var result = await _userManager.CreateAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                //await _userManager.AddToRoleAsync(user, "user");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                //if (_userManager.Options.SignIn.RequireConfirmedAccount)
                //{
                //    return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                //}
                //else
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, GetDescription(error.Code));
            }
        }

        // If we got this far, something failed, redisplay form
        return Page();
    }

    private UserModel CreateUser()
    {
        try
        {

           return Activator.CreateInstance<UserModel>();
        }
        catch
        {
            throw new InvalidOperationException($"Can't create an instance of '{nameof(UserModel)}'. " +
                $"Ensure that '{nameof(User)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
        }
    }

    private IUserEmailStore<UserModel> GetEmailStore()
    {
        if (!_userManager.SupportsUserEmail)
        {
            throw new NotSupportedException("The default UI requires a user store with email support.");
        }
        return (IUserEmailStore<UserModel>)_userStore;
    }

    private string GetDescription(string IdentityErrorCode) 
    {
        string description = "";

        switch (IdentityErrorCode)
        {
            case "PasswordRequiresNonAlphanumeric":
                description = Register.PasswordRequiresNonAlphanumeric;
                break;

            case "PasswordRequiresLower":
                description = Register.PasswordRequiresLower;
                break;

            case "PasswordRequiresUpper":
                description = Register.PasswordRequiresUpper;
                break;

            default:
                description = "Ошибка";
                break;
        }

        return description;
    }
}

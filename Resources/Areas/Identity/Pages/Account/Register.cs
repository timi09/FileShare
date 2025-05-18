using System.Resources;
using System.Globalization;

namespace FileShare.Resources.Areas.Identity.Pages.Account
{
    // Класс для доступа к ресурсам Register.resx
    public static class Register
    {
        private static readonly ResourceManager _resourceManager =
            new ResourceManager("FileShare.Resources.Areas.Identity.Pages.Account.Register", typeof(Register).Assembly);

        // Свойства для доступа к строкам
        public static string EmailValid => _resourceManager.GetString("EmailValid", CultureInfo.CurrentCulture) ?? "Email is invalid";
        public static string EmailFieldRequired => _resourceManager.GetString("EmailFieldRequired", CultureInfo.CurrentCulture) ?? "Email is required";
        public static string PasswordFieldRequired => _resourceManager.GetString("PasswordFieldRequired", CultureInfo.CurrentCulture) ?? "Password is required";
        public static string PasswordLength => _resourceManager.GetString("PasswordLength", CultureInfo.CurrentCulture) ?? "Password must be between {2} and {1} characters";
        public static string PasswordCompare => _resourceManager.GetString("PasswordCompare", CultureInfo.CurrentCulture) ?? "Passwords do not match";
        public static string PasswordConfirmRequired => _resourceManager.GetString("PasswordConfirmRequired", CultureInfo.CurrentCulture) ?? "Password confirm required";

        public static string PasswordRequiresLower => _resourceManager.GetString("PasswordRequiresLower", CultureInfo.CurrentCulture) ?? "PasswordRequiresLower";
        public static string PasswordRequiresNonAlphanumeric => _resourceManager.GetString("PasswordRequiresNonAlphanumeric", CultureInfo.CurrentCulture) ?? "PasswordRequiresNonAlphanumeric";
        public static string PasswordRequiresUpper => _resourceManager.GetString("PasswordRequiresUpper", CultureInfo.CurrentCulture) ?? "PasswordRequiresUpper";
    }
}
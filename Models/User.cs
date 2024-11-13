using Microsoft.AspNetCore.Identity;

namespace FileShare.Models;

public class UserModel : IdentityUser
{
    public List<FileModel> Files { get; set; }
}

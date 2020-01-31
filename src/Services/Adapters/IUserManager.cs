using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace safnet.Identity.Api.Services.Adapters
{
    public interface IUserManager
    {
        Task<IdentityResult> CreateAsync(IdentityUser user);
        Task<IdentityResult> UpdateAsync(IdentityUser user);
        Task<IdentityResult> DeleteAsync(IdentityUser user);
        Task<IdentityUser> FindByIdAsync(string userId);
        Task<IdentityUser> FindByNameAsync(string userName);
        Task<IdentityResult> CreateAsync(IdentityUser user, string password);
        Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword,
            string newPassword);
        Task<IdentityResult> ResetPasswordAsync(IdentityUser user, string token, string newPassword);
    }
}

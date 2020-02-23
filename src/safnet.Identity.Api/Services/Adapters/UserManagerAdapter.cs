using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using safnet.Common.GenericExtensions;

namespace safnet.Identity.Api.Services.Adapters
{
    [ExcludeFromCodeCoverage]
    public class UserManagerAdapter : IUserManager
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UserManagerAdapter(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager.MustNotBeNull(nameof(userManager));
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            user.MustNotBeNull(nameof(user));

            return await _userManager.CreateAsync(user);
        }

        public async Task<IdentityResult> UpdateAsync(IdentityUser user)
        {
            user.MustNotBeNull(nameof(user));

            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> DeleteAsync(IdentityUser user)
        {
            user.MustNotBeNull(nameof(user));

            return await _userManager.DeleteAsync(user);
        }

        public async Task<IdentityUser> FindByIdAsync(string userId)
        {
            userId.MustNotBeNull(nameof(userId));

            return await _userManager.FindByIdAsync(userId);
        }

        public async Task<IdentityUser> FindByNameAsync(string userName)
        {
            userName.MustNotBeNull(nameof(userName));

            return await _userManager.FindByNameAsync(userName);
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user, string password)
        {
            user.MustNotBeNull(nameof(user));
            password.MustNotBeNull(nameof(password));

            return await _userManager.CreateAsync(user, password);
        }

        public async Task<IdentityResult> ChangePasswordAsync(IdentityUser user, string currentPassword,
            string newPassword)
        {
            user.MustNotBeNull(nameof(user));
            currentPassword.MustNotBeNull(nameof(currentPassword));
            newPassword.MustNotBeNull(nameof(newPassword));

            return await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        public async Task<IdentityResult> ResetPasswordAsync(IdentityUser user, string token, string newPassword)
        {
            user.MustNotBeNull(nameof(user));
            token.MustNotBeNull(nameof(token));
            newPassword.MustNotBeNull(nameof(newPassword));

            return await _userManager.ResetPasswordAsync(user, token, newPassword);
        }
    }
}

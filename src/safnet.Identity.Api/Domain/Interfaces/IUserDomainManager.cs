using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlightNode.Identity.Domain.Interfaces
{
    public interface IUserDomainManager
    {
        IEnumerable<UserModel> FindAll();
        UserModel FindById(int id);
        UserModel Create(UserModel input);
        UserModel CreatePending(UserModel input);
        void Update(UserModel input);
        void ChangePassword(int id, PasswordModel change);
        void AdministrativePasswordChange(int userId, string newPassword);
        IEnumerable<PendingUserModel> FindAllPending();
        Task Approve(List<int> list);
        Task<bool> RequestPasswordChange(string emailAddress);
        Task<ChangeForgottenPasswordResult> ChangeForgottenPassword(string token, ChangePasswordModel input);
    }

}

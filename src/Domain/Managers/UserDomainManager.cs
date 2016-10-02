using FlightNode.Common.Exceptions;
using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace FlightNode.Identity.Domain.Logic
{
    /// <summary>
    /// Domain / business logic for Users.
    /// </summary>
    public class UserDomainManager : IUserDomainManager
    {

        public static readonly string PendingUserEmailSubject = Properties.Settings.Default.SiteName + " pending user registration received";
        public const string PendingUserEmailBodyPattern = @"Thank you for creating a new account at {0}. Your account will remain in a pending state until an administrator approves your registration, at which point you will receive an e-mail notification to alert you to the change in status.

Username: {1}
Password: {2}

Please visit the website's Contact form to submit any questions to the administrators.
";
        public static readonly string ApprovedEmailSubject = Properties.Settings.Default.SiteName + " user registration has been approved";
        public const string AccountApprovedEmailBodyPattern = @"Your account registration at {0} has been approved, and you can now start entering data. 

Username: {1}
";
        public static readonly string PasswordChangeRequestSubject = Properties.Settings.Default.SiteName + " Password Change Request";
        public const string PasswordChangeRequestBodyPattern = @"Please use the followign link to change your password. You have 24 hours to reset your password, after which this URL will expire. You can request a new password again at that point to generate a new email.

{0}";

        private IUserPersistence _userManager;
        private IEmailFactory _emailFactory;


        /// <summary>
        /// Creates a new instance of <see cref="UserDomainManager"/>.
        /// </summary>
        /// <param name="manager">Instance of <see cref="Interfaces.IUserPersistence"/></param>
        /// <param name="emailFactory">Instance of <see cref="IEmailFactory"/></param>
        public UserDomainManager(IUserPersistence manager, IEmailFactory emailFactory)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }
            if (emailFactory == null)
            {
                throw new ArgumentNullException("emailFactory");
            }

            _userManager = manager;
            _emailFactory = emailFactory;
        }


        public IEnumerable<UserModel> FindAll()
        {
            return _userManager.Users
                .Where(x => x.Active == User.StatusActive)
                .ToList()
                .Select(Map);
        }

        public IEnumerable<PendingUserModel> FindAllPending()
        {
            return _userManager.Users
                .Where(x => x.Active == User.StatusPending)
                .ToList()
                .Select(x => new PendingUserModel
                {
                    DisplayName = x.DisplayName,
                    Email = x.Email,
                    PrimaryPhoneNumber = x.PhoneNumber,
                    SecondaryPhoneNumber = x.MobilePhoneNumber,
                    UserId = x.Id
                });
        }

        public UserModel FindById(int id)
        {
            UserModel output = null;

            var record = _userManager.FindByIdAsync(id).Result ?? new User();

            output = Map(record);
            if (record.Id > 0)
            {
                // Originally supported multiple roles, but now simplifying to support only one. Hence the pluralization.
                var roles = _userManager.GetRolesAsync(id).Result;
                output.Role = (int) Enum.Parse(typeof(RoleEnum), roles.FirstOrDefault());
            }

            return output;
        }

        private UserModel Map(User input)
        {
            return new UserModel
            {
                Email = input.Email,
                SecondaryPhoneNumber = input.MobilePhoneNumber,
                Password = string.Empty,
                PrimaryPhoneNumber = input.PhoneNumber,
                UserId = input.Id,
                UserName = input.UserName,
                GivenName = input.GivenName,
                FamilyName = input.FamilyName,
                LockedOut = input.LockoutEnabled,
                Active = input.Active == User.StatusActive,
                MailingAddress = input.MailingAddress,
                City = input.City,
                State = input.State,
                County = input.County,
                ZipCode = input.ZipCode
            };
        }


        public void Update(UserModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }


            // TODO: transaction management. Should not be able to update user and remove old
            // roles, if there is a problem with saving new ones.

            var record = _userManager.FindByIdAsync(input.UserId).Result;

            record = Map(input, record);

            var result = _userManager.UpdateAsync(record).Result;
            if (result.Succeeded)
            {
                var existingRoles = _userManager.GetRolesAsync(input.UserId).Result;
                result = _userManager.RemoveFromRolesAsync(input.UserId, existingRoles.ToArray()).Result;
                if (!result.Succeeded)
                {
                    throw UserException.FromMultipleMessages(result.Errors);
                }
                
                result = _userManager.AddToRolesAsync(input.UserId, ParseRoleName(input)).Result;
                if (!result.Succeeded)
                {
                    throw UserException.FromMultipleMessages(result.Errors);
                }
            }
            else
            {
                throw UserException.FromMultipleMessages(result.Errors);
            }
        }

        private string ParseRoleName(UserModel input)
        {
            return ((RoleEnum)input.Role).ToString();
        }



        public UserModel Create(UserModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var record = Map(input);

            input.UserId = Create(record, ParseRoleName(input), input.Password);

            return input;
        }

        public UserModel CreatePending(UserModel input)
        {
            var record = Map(input);

            input.UserId = SavePendingUserRecord(input, record);

            SendPendingUserEmail(record, input);

            return input;
        }

        private void SendPendingUserEmail(User record, UserModel dto)
        {
            var body = string.Format(PendingUserEmailBodyPattern, Properties.Settings.Default.IssuerUrl, record.UserName, dto.Password);
            var message = new NotificationModel(record.FormattedEmail, PendingUserEmailSubject, body);

            _emailFactory.CreateNotifier()
                .SendAsync(message);
        }

        private int SavePendingUserRecord(UserModel input, User record)
        {
            // Don't trust the client to provide these three important values!
            record.LockoutEnabled = true;
            record.Active = User.StatusPending;
            
            return Create(record, RoleEnum.Reporter.ToString() ,input.Password);
        }

        private int Create(User record, string role, string password)
        {
            var result = _userManager.CreateAsync(record, password).Result;
            if (result.Succeeded)
            {
                result = _userManager.AddToRolesAsync(record.Id, role).Result;

                if (result.Succeeded)
                {
                    return record.Id;
                }
                else
                {
                    throw UserException.FromMultipleMessages(result.Errors);
                }
            }
            else
            {
                throw UserException.FromMultipleMessages(result.Errors);
            }
        }

        private User Map(UserModel input, User record = null)
        {
            record = record ?? new User();
            record.Active = input.Active ? User.StatusActive : User.StatusInactive;
            record.Email = input.Email;
            record.FamilyName = input.FamilyName;
            record.GivenName = input.GivenName;
            record.MobilePhoneNumber = input.SecondaryPhoneNumber;
            record.PhoneNumber = input.PrimaryPhoneNumber;
            record.UserName = input.UserName;
            record.LockoutEnabled = input.LockedOut;
            record.County = input.County;
            record.MailingAddress = input.MailingAddress;
            record.City = input.City;
            record.State = input.State;
            record.ZipCode = input.ZipCode;

            return record;
        }

        public void ChangePassword(int id, PasswordModel change)
        {
            var result = _userManager.ChangePasswordAsync(id, change.CurrentPassword, change.NewPassword).Result;
            if (!result.Succeeded)
            {
                throw UserException.FromMultipleMessages(result.Errors);
            }
        }

        public void AdministrativePasswordChange(int userId, string newPassword)
        {
            var token = _userManager.GeneratePasswordResetTokenAsync(userId).Result;
            var result = _userManager.ResetPasswordAsync(userId, token, newPassword).Result;
            if (!result.Succeeded)
            {
                throw UserException.FromMultipleMessages(result.Errors);
            }
        }

        public async Task Approve(List<int> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            foreach (var id in ids)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user != null)
                {
                    await ApproveSingleUser(user);
                    await SendApprovalEmail(user);
                }
                // Ignore else case - likely implies some manipulation of the input.
                // Otherwise, doesn't really matter. The record will show up in the list again.
            }
        }

        private async Task SendApprovalEmail(User user)
        {
            var message = new NotificationModel(
                user.FormattedEmail,
                ApprovedEmailSubject,
                string.Format(CultureInfo.InvariantCulture, AccountApprovedEmailBodyPattern, Properties.Settings.Default.SiteName, user.UserName)
                );

            await _emailFactory.CreateNotifier()
                .SendAsync(message);
        }

        private async Task ApproveSingleUser(User user)
        {
            user.Active = User.StatusActive;
            user.LockoutEnabled = false;

            await _userManager.UpdateAsync(user);
        }

        /// <summary>
        /// Sends an e-mail containing a password reset token.
        /// </summary>
        /// <param name="emailAddress">Existing user's e-mail address</param>
        /// <returns>
        /// True if the e-mail address was found
        /// False otherwise
        /// </returns>
        public async Task<bool> RequestPasswordChange(string emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException(nameof(emailAddress));
            }
            if (string.IsNullOrWhiteSpace(emailAddress))
            {
                throw new ArgumentException(nameof(emailAddress) + " cannot be an empty string", emailAddress);
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Email == emailAddress);
            if (user == null)
            {
                return false;
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user.Id);
            token = HttpUtility.UrlEncode(token);

            var body = string.Format(CultureInfo.InvariantCulture, PasswordChangeRequestBodyPattern, Properties.Settings.Default.PasswordChangeBaseUrl + "?token=" + token);

            var message = new NotificationModel(
               user.FormattedEmail,
               PasswordChangeRequestSubject,
               body
               );

            await _emailFactory.CreateNotifier()
                .SendAsync(message);


            return true;
        }

        public async Task<ChangeForgottenPasswordResult> ChangeForgottenPassword(string token, ChangePasswordModel input)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException(nameof(token) + " cannot be an empty string", token);
            }
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var user = _userManager.Users.FirstOrDefault(u => u.Email == input.EmailAddress);
            if (user == null)
            {
                return ChangeForgottenPasswordResult.UserDoesNotExist;
            }

            var result = await _userManager.ResetPasswordAsync(user.Id, token, input.Password);



            if (result.Succeeded)
            {
                return ChangeForgottenPasswordResult.Happy;
            }
            else if (result.Errors.First() == "Invalid token.")
            {
                return ChangeForgottenPasswordResult.BadToken;
            }
            else if (result.Errors.First().StartsWith("Passwords must"))
            {
                return ChangeForgottenPasswordResult.InvalidPassword;
            }


            throw new InvalidOperationException("Reset password request resulted in error: " + result.Errors.First());
        }
    }

}

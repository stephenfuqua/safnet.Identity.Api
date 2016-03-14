using FlightNode.Common.BaseClasses;
using FlightNode.Common.Exceptions;
using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightNode.Identity.Domain.Logic
{
    /// <summary>
    /// Domain / business logic for Users.
    /// </summary>
    public class UserDomainManager : DomainLogic, IUserDomainManager
    {

        public static readonly string PendingUserEmailSubject = Properties.Settings.Default.SiteName + " pending user registration received";
        public const string PendingUserEmailBodyPattern = @"Thank you for creating a new account at {0}. Your account will remain in a pending state until an administrator approves your registration, at which point you will receive an e-mail notification to alert you to the change in status.

Username: {1}
Password: {2}

Please visit the website's Contact form to submit any questions to the administrators.
";

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
                .Where(x => x.Active == User.STATUS_ACTIVE)
                .ToList()
                .Select(Map);
        }

        public IEnumerable<PendingUserModel> FindAllPending()
        {
            return _userManager.Users
                .Where(x => x.Active == User.STATUS_PENDING)
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
                output.Roles.AddRange(_userManager.GetRolesAsync(id).Result);
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
                Active = input.Active == User.STATUS_ACTIVE,
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

                result = _userManager.AddToRolesAsync(input.UserId, input.Roles.ToArray()).Result;
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



        public UserModel Create(UserModel input)
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            var record = Map(input);

            input.UserId = Create(record, input.Roles.ToArray(), input.Password);

            return input;
        }

        public UserModel CreatePending(UserModel input)
        {
            var record = Map(input);

            input.UserId = SavePendingUserRecord(input, record);

            SendPendingUserEmail(input);

            return input;
        }

        private void SendPendingUserEmail(UserModel input)
        {
            var to = string.Format("{0} {1} <{2}>", input.GivenName, input.FamilyName, input.Email);
            var body = string.Format(PendingUserEmailBodyPattern, Properties.Settings.Default.IssuerUrl, input.UserName, input.Password);
            var message = new NotificationModel(to, PendingUserEmailSubject, body);

            _emailFactory.CreateNotifier()
                .Send(message);
        }

        private int SavePendingUserRecord(UserModel input, User record)
        {
            // Don't trust the client to provide these three important values!
            record.LockoutEnabled = true;
            record.Active = User.STATUS_PENDING;
            var roles = new[] { "Reporter" };

            return Create(record, roles, input.Password);
        }

        private int Create(User record, string[] roles, string password)
        {
            var result = _userManager.CreateAsync(record, password).Result;
            if (result.Succeeded)
            {
                result = _userManager.AddToRolesAsync(record.Id, roles).Result;

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
            record.Active = input.Active ? User.STATUS_ACTIVE : User.STATUS_INACTIVE;
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

        public void Approve(List<int> ids)
        {
            if (ids == null)
            {
                throw new ArgumentNullException("ids");
            }

            foreach (var id in ids)
            {
                var user = _userManager.FindByIdAsync(id).Result;

                if (user != null)
                {
                    user.Active = User.STATUS_ACTIVE;
                    user.LockoutEnabled = false;

                    var updateResult = _userManager.UpdateAsync(user).Result;
                }
                // Ignore else case - likely implies some manipulation of the input.
                // Otherwise, doesn't really matter. The record will show up in the list again.
            }
        }
    }
}

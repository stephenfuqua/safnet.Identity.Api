using FlightNode.Common.BaseClasses;
using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FlightNode.Identity.Domain.Logic
{

    public class UserDomainManager : DomainLogic, IUserDomainManager
    {

        private Interfaces.IUserPersistence _userManager;

        public UserDomainManager(Interfaces.IUserPersistence manager)
        {
            if (manager == null)
            {
                throw new ArgumentNullException("manager");
            }

            _userManager = manager;
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

            // Don't trust the client to provide these three important values!
            record.LockoutEnabled = true;
            record.Active = User.STATUS_PENDING;
            var roles = new[] { "Reporter" };

            input.UserId = Create(record, roles, input.Password);

            return input;
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

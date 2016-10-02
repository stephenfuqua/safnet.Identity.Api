using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{


    public class UpdateUser : Fixture
    {
        [Fact]
        public void NullObjectNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                BuildSystem().Update(null);
            });
        }

        private void RunTest()
        {
            var input = new UserModel
            {
                Email = Email,
                FamilyName = FamilyName,
                GivenName = GivenName,
                Password = Password,
                PrimaryPhoneNumber = PrimaryPhoneNumber,
                SecondaryPhoneNumber = SecondaryPhoneNumber,
                UserName = UserName,
                UserId = UserId,
                LockedOut = LockedOut,
                Active = Active,
                City = City,
                State = State,
                ZipCode = ZipCode,
                County = County,
                MailingAddress = Mailing
            };
            input.Role = NewRole;

            BuildSystem().Update(input);
        }



        private void ExpectSuccessfulSaveOfUser()
        {
            MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                                .Returns(CreateSuccessResult());
        }

        private void ExpectSuccessfulSaveOfRoles()
        {
            MockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                                .Returns(CreateSuccessResult());
        }

        private void ConfirmFieldMapping(Func<User, bool> assert)
        {

            ExpectSuccessfulSaveOfRoles();
            ExpectSuccessfulSaveOfUser();
            ExpectToQueryForExistingUserRecord();
            ExpectToRemoveOldRoles();
            ExpectToSaveNewRoles();
            ExpectUserIsAlreadyInARole();

            RunTest();

            MockUserManager.Verify(x => x.UpdateAsync(It.Is<User>(y => assert(y))));
        }
        [Fact]
        public void ConfirmUserNameIsMapped()
        {

            ConfirmFieldMapping(x => x.UserName == UserName);
        }

        [Fact]
        public void ConfirmGivenNameIsMapped()
        {

            ConfirmFieldMapping(x => x.GivenName == GivenName);
        }

        [Fact]
        public void ConfirmFamilyNameIsMapped()
        {
            ConfirmFieldMapping(x => x.FamilyName == FamilyName);
        }

        [Fact]
        public void ConfirmPrimaryPhoneNumberIsMapped()
        {
            ConfirmFieldMapping(x => x.PhoneNumber == PrimaryPhoneNumber);
        }

        [Fact]
        public void ConfirmSecondaryPhoneNumberIsMapped()
        {
            ConfirmFieldMapping(x => x.MobilePhoneNumber == SecondaryPhoneNumber);
        }

        [Fact]
        public void ConfirmEmailPhoneNumberIsMapped()
        {
            ConfirmFieldMapping(x => x.Email == Email);
        }


        [Fact]
        public void ConfirmUserIsLockedOut()
        {
            ConfirmFieldMapping(x => x.LockoutEnabled == LockedOut);
        }

        [Fact]
        public void ConfirmActiveStatus()
        {
            ConfirmFieldMapping(x => x.Active == ActiveString);
        }


        [Fact]
        public void ConfirmCountyIsMapped()
        {
            ConfirmFieldMapping(x => x.County == County);
        }


        [Fact]
        public void ConfirmMailingAddressIsMapped()
        {
            ConfirmFieldMapping(x => x.MailingAddress == Mailing);
        }


        [Fact]
        public void ConfirmCityIsMapped()
        {
            ConfirmFieldMapping(x => x.City == City);
        }

        [Fact]
        public void ConfirStateIsMapped()
        {
            ConfirmFieldMapping(x => x.State == State);
        }

        [Fact]
        public void ConfirmZipCodeIsMapped()
        {
            ConfirmFieldMapping(x => x.ZipCode == ZipCode);
        }


        [Fact]
        public void ConfirmSearchForExistingUserFirst()
        {
            SetupSuccessfulDatabaseExpectations();

            RunTest();


            MockUserManager.Verify(x => x.FindByIdAsync(It.Is<int>(y => y == UserId)));
        }

        private void SetupSuccessfulDatabaseExpectations()
        {
            ExpectToQueryForExistingUserRecord();
            ExpectSuccessfulSaveOfUser();
            ExpectUserIsAlreadyInARole();
            ExpectToRemoveOldRoles();
            ExpectToSaveNewRoles();
        }

        [Fact]
        public void ConfirmSearchForExistingRoles()
        {
            SetupSuccessfulDatabaseExpectations();

            RunTest();

            MockUserManager.Verify(x => x.GetRolesAsync(It.Is<int>(y => y == UserId)));
        }


        [Fact]
        public void ConfirmRemovalOfOldRoleForUser()
        {
            SetupSuccessfulDatabaseExpectations();

            RunTest();

            MockUserManager.Verify(x => x.RemoveFromRolesAsync(It.Is<int>(y => y == UserId),
                                                               It.IsAny<string[]>()));
        }

        [Fact]
        public void ConfirmRemovalOfOldRoleByName()
        {
            SetupSuccessfulDatabaseExpectations();

            RunTest();

            MockUserManager.Verify(x => x.RemoveFromRolesAsync(It.IsAny<int>(),
                                                               It.Is<string[]>(y => y[0] == OldRoleString)));
        }


        [Fact]
        public void ConfirmaddNewRoleForUser()
        {
            SetupSuccessfulDatabaseExpectations();

            RunTest();

            MockUserManager.Verify(x => x.AddToRolesAsync(It.Is<int>(y => y == UserId),
                                                          It.IsAny<string[]>()));
        }

        [Fact]
        public void ConfirmaddNewRoleByName()
        {
            SetupSuccessfulDatabaseExpectations();

            RunTest();

            MockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<int>(),
                                                          It.Is<string[]>(y => y[0] == NewRoleString)));
        }

        private void ExpectToRemoveOldRoles()
        {
            MockUserManager.Setup(x => x.RemoveFromRolesAsync(It.IsAny<int>(),
                                                                              It.IsAny<string[]>()))
                                                .Returns(CreateSuccessResult());
        }

        private void ExpectToSaveNewRoles()
        {
            MockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(),
                                                                         It.IsAny<string[]>()))
                                                .Returns(CreateSuccessResult());
        }

        private void ExpectUserIsAlreadyInARole()
        {
            MockUserManager.Setup(x => x.GetRolesAsync(It.IsAny<int>()))
                                .ReturnsAsync(new List<string>() { OldRoleString });
        }

        private void ExpectToQueryForExistingUserRecord()
        {
            MockUserManager.Setup(x => x.FindByIdAsync(It.IsAny<int>()))
                                .Returns(Task.Run(() => new User()));
        }

        [Fact]
        public void ConfirmHandlingOfErrorWhenSavingUpdatedUserRecord()
        {
            ExpectToQueryForExistingUserRecord();

            MockUserManager.Setup(x => x.UpdateAsync(It.IsAny<User>()))
                .Returns((User actual) =>
                {
                    actual.Id = UserId;

                    return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                });

            Assert.Throws<UserException>(() => RunTest());
        }

        [Fact]
        public void ConfirmHandlingOfErrorWhenRemovingExistingRoles()
        {
            const string badStuffHappened = "Bad stuff happened";

            ExpectToQueryForExistingUserRecord();
            ExpectSuccessfulSaveOfUser();
            ExpectUserIsAlreadyInARole();


            MockUserManager.Setup(x => x.RemoveFromRolesAsync(It.IsAny<int>(),
                                                              It.IsAny<string[]>()))
                        .ReturnsAsync(IdentityResult.Failed(badStuffHappened));


            // Act & assert
            Assert.Throws<UserException>(() => RunTest());
        }

        [Fact]
        public void ConfirmHandlingOfErrorWhenRemovingAddingNewRoles()
        {
            const string badStuffHappened = "Bad stuff happened";

            ExpectToQueryForExistingUserRecord();
            ExpectSuccessfulSaveOfUser();
            ExpectUserIsAlreadyInARole();
            ExpectToRemoveOldRoles();


            MockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(),
                                                         It.IsAny<string[]>()))
                        .ReturnsAsync(IdentityResult.Failed(badStuffHappened));


            // Act & assert
            Assert.Throws<UserException>(() => RunTest());
        }
    }
}

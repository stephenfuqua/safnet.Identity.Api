using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{


    public class CreateUser : Fixture
    {
        [Fact]
        public void NullObjectNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                BuildSystem().Create(null);
            });
        }

        private UserModel RunTest()
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
                LockedOut = LockedOut,
                Active = Active,
                County = County,
                MailingAddress = Mailing,
                City = City,
                State = State,
                ZipCode = ZipCode,
                Role = (int)RoleEnum.Administrator
            };

            return BuildSystem().Create(input);
        }

        [Fact]
        public void ConfirmUserIdIsSetInReturnObject()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            Assert.Equal(UserId, RunTest().UserId);
        }

        private void SetupMockExpectationsForSuccessfulSaveOfUserAndRoles()
        {
            ExpectSuccessfulSaveOfRoles();

            ExpectSuccessfulSaveOfUser();
        }

        private void ExpectSuccessfulSaveOfUser()
        {
            MockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                                .Returns((User actual, string p) =>
                                {
                                    actual.Id = UserId;

                                    return CreateSuccessResult();
                                });
        }

        private void ExpectSuccessfulSaveOfRoles()
        {
            MockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                                                .Returns(Task.Run(() => SuccessResult.Create()));
        }

        [Fact]
        public void ConfirmUserIdIsMappedToRole()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            RunTest();

            MockUserManager.Verify(x => x.AddToRolesAsync(It.Is<int>(y => y == UserId), It.IsAny<string[]>()));
        }

        [Fact]
        public void ConfirmThereAreNoRoles()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            RunTest();

            MockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<int>(), "Administrator"));
        }

        [Fact]
        public void ConfirmPasswordIsMapped()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            RunTest();

            MockUserManager.Verify(x => x.CreateAsync(It.IsAny<User>(), It.Is<string>(y => y == Password)));
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
        public void ConfirmPrimaryPhoneIsMapped()
        {
            ConfirmFieldMapping(x => x.PhoneNumber == PrimaryPhoneNumber);
        }

        [Fact]
        public void ConfirmSecondaryPhoneIsMapped()
        {
            ConfirmFieldMapping(x => x.MobilePhoneNumber == SecondaryPhoneNumber);
        }

        [Fact]
        public void ConfirmUserNameIsMapped()
        {
            ConfirmFieldMapping(x => x.UserName == UserName);
        }

        [Fact]
        public void ConfirmEmailIsMapped()
        {
            ConfirmFieldMapping(x => x.Email == Email);
        }

        [Fact]
        public void ConfirmLockedOutIsMapped()
        {
            ConfirmFieldMapping(x => x.LockoutEnabled == LockedOut);
        }

        [Fact]
        public void ConfirmActiveIsMapped()
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
        public void ConfirmCityMapped()
        {
            ConfirmFieldMapping(x => x.City == City);
        }


        [Fact]
        public void ConfirmStateIsMapped()
        {
            ConfirmFieldMapping(x => x.State == State);
        }


        [Fact]
        public void ConfirmZipCodeIsMapped()
        {
            ConfirmFieldMapping(x => x.ZipCode == ZipCode);
        }


        private void ConfirmFieldMapping(Func<User, bool> assert)
        {

            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            RunTest();

            MockUserManager.Verify(x => x.CreateAsync(It.Is<User>(y => assert(y)), It.IsAny<string>()));
        }


        [Fact]
        public void ConfirmExceptionIfRolesDoNotSave()
        {
            MockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(Task.Run(() => IdentityResult.Failed("asdfasd")));

            ExpectSuccessfulSaveOfUser();

            Assert.Throws<UserException>(() => RunTest());
        }

        [Fact]
        public void ConfirmErrorHandling()
        {
            MockUserManager.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .Returns((User actual, string p) =>
                {
                    actual.Id = UserId;

                    return Task.Run(() => new IdentityResult(new[] { "something bad happened" }));
                });


            Assert.Throws<UserException>(() => RunTest().UserId);
        }
    }
}

using FlightNode.Common.Exceptions;
using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Logic;
using FlightNode.Identity.Services.Models;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{


    public class CreatePendingUser : Fixture
    {
        [Fact]
        public void NullObjectNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                BuildSystem().Create(null);
            });
        }

        private void SetupMockExpectationsForSuccessfulSaveOfUserAndRoles()
        {
            ExpectToSendEmail();

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

        private void ConfirmFieldMapping(Func<User, bool> assert)
        {
            ExpectToSendEmail();
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            RunTest();

            MockUserManager.Verify(x => x.CreateAsync(It.Is<User>(y => assert(y)), It.IsAny<string>()));
        }


        private UserModel RunTest()
        {

            //
            // Arrange
            var input = new UserModel
            {
                Email = Email,
                FamilyName = FamilyName,
                GivenName = GivenName,
                Password = Password,
                PrimaryPhoneNumber = PrimaryPhoneNumber,
                SecondaryPhoneNumber = SecondaryPhoneNumber,
                UserName = UserName,
                County = County,
                City = City,
                MailingAddress = Mailing,
                State = State,
                ZipCode = ZipCode
            };

            //
            // Act
            return BuildSystem().CreatePending(input);
        }

        [Fact]
        public void ConfirmSendsEmailToNewUserWithCorrectNameAndEmail()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();
            var emailFactoryMock = ExpectToSendEmail();

            RunTest();

            VerifyEmailParameter(emailFactoryMock, y => y.To == GivenName + " " + FamilyName + " <" + Email + ">");
        }

        [Fact]
        public void ConfirmSendsEmailToNewUserWithCorrectSubject()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();
            var emailFactoryMock = ExpectToSendEmail();

            RunTest();

            VerifyEmailParameter(emailFactoryMock, y => y.Subject == UserDomainManager.PendingUserEmailSubject);
        }

        [Fact]
        public void ConfirmSendsEmailToNewUserWithCorrectMessage()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();
            var emailFactoryMock = ExpectToSendEmail();
            var expected = @"Thank you for creating a new account at http://localhost. Your account will remain in a pending state until an administrator approves your registration, at which point you will receive an e-mail notification to alert you to the change in status.

Username: " + UserName + @"
Password: " + Password + @"

Please visit the website's Contact form to submit any questions to the administrators.
";

            RunTest();

            VerifyEmailParameter(emailFactoryMock, y => y.Body == expected);
        }

        private static void VerifyEmailParameter(Mock<IEmailNotifier> emailFactoryMock, Func<NotificationModel, bool> assert)
        {
            emailFactoryMock.Verify(x => x.SendAsync(It.Is<NotificationModel>(y => assert(y))));
        }


        [Fact]
        public void ConfirmUserIdIsSetInReturnObject()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            var result = RunTest();

            Assert.Equal(UserId, result.UserId);
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
        public void ConfirmStatusIsPending()
        {
            ConfirmFieldMapping(x => x.Active == PendingString);
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
        public void ConfirmRoleIsSetForNewUser()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            var result = RunTest();

            MockUserManager.Verify(x => x.AddToRolesAsync(It.Is<int>(y => y == UserId), It.IsAny<string[]>()));
        }

        [Fact]
        public void ConfirmRoleIsReporter()
        {
            SetupMockExpectationsForSuccessfulSaveOfUserAndRoles();

            var result = RunTest();

            MockUserManager.Verify(x => x.AddToRolesAsync(It.IsAny<int>(), It.Is<string[]>(y => y[0] == "Reporter")));
        }

        [Fact]
        public void ConfirmExceptionIfRolesDoNotSave()
        {
            MockUserManager.Setup(x => x.AddToRolesAsync(It.IsAny<int>(), It.IsAny<string[]>()))
                .Returns(Task.Run(() => SuccessResult.Failed("asdfasd")));

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


            Assert.Throws<UserException>(() => RunTest());
        }
    }
}

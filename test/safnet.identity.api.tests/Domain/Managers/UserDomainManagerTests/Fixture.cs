using FlightNode.Common.Utility;
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Domain.Logic;
using Microsoft.AspNet.Identity;
using Moq;
using System;
using System.Threading.Tasks;

namespace FlightNode.Identity.UnitTests.Domain.Managers.UserDomainManagerTests
{

    public class Fixture : IDisposable
    {
        protected const string GivenName = "José";
        protected const string FamilyName = "Jalapeño";
        protected const string PrimaryPhoneNumber = "(512) 555-2391";
        protected const string SecondaryPhoneNumber = "(651) 234-2349";
        protected const string UserName = "josej";
        protected const string Password = "bien gracias, y tú?";
        protected const string Email = "jose@jalapenos.com";
        protected const int UserId = 2342;
        protected const bool LockedOut = true;
        protected const bool Active = true;
        protected const string ActiveString = "active";
        protected const string PendingString = "pending";
        protected const string County = "County";
        protected const string Mailing = "3334 Park Ave";
        protected const string City = "Ctyyy";
        protected const string State = "TX";
        protected const string ZipCode = "23423";
        protected const int OldRole = 1;
        protected static readonly string OldRoleString = RoleEnum.Administrator.ToString();
        protected const int NewRole = 2;
        protected static readonly string NewRoleString = RoleEnum.Reporter.ToString();
        protected const string RoleAdministrator = "Administrator";

        protected MockRepository MockRepository = new MockRepository(MockBehavior.Strict);
        protected Mock<IUserPersistence> MockUserManager;
        protected Mock<IEmailFactory> EmailFactoryMock;
        protected Mock<IEmailNotifier> EmailNotifierMock;

        public Fixture()
        {
            MockUserManager = MockRepository.Create<IUserPersistence>();
            EmailFactoryMock = MockRepository.Create<IEmailFactory>();
            EmailNotifierMock = MockRepository.Create<IEmailNotifier>();
        }


        protected UserDomainManager BuildSystem()
        {
            return new UserDomainManager(MockUserManager.Object, EmailFactoryMock.Object);
        }

        public void Dispose()
        {
            MockUserManager.VerifyAll();
        }


        protected Mock<IEmailNotifier> ExpectToSendEmail()
        {
            var notifierMock = this.MockRepository.Create<IEmailNotifier>();
            notifierMock.Setup(x => x.SendAsync(It.IsAny<NotificationModel>()))
                .Returns(Task.Run(() => SuccessResult.Create()));


            this.EmailFactoryMock.Setup(x => x.CreateNotifier())
                .Returns(notifierMock.Object);

            return notifierMock;
        }

        public static Task<IdentityResult> CreateSuccessResult()
        {
            return Task.Run(() => SuccessResult.Create());
        }
    }
}

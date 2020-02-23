using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Interfaces;
using FligthNode.Identity.Services.Controllers;
using log4net;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Web.Http;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{

    public class Fixture : IDisposable
    {
        public class UsersControllerTss : UsersController
        {
            public UsersControllerTss(IUserDomainManager manager) : base(manager)
            {
                base.ManagerWithTokenSetup = manager;
            }
        }

        protected MockRepository Repository = new MockRepository(MockBehavior.Strict);
        protected Mock<IUserDomainManager> MockManager;
        protected Mock<ILog> MockLogger;
        protected const string baseLocation = "http://localhost/v1/users";

        protected Fixture()
        {
            MockManager = Repository.Create<IUserDomainManager>();
            MockLogger = Repository.Create<ILog>();
        }

        protected UsersControllerTss BuildSystem()
        {
            var controller = new UsersControllerTss(MockManager.Object);
            controller.Logger = MockLogger.Object;

            controller.Request = new HttpRequestMessage() { RequestUri = new Uri(baseLocation) };
            controller.Configuration = new HttpConfiguration();

            return controller;
        }

        public void Dispose()
        {
            Repository.VerifyAll();
        }



        protected DomainValidationException CreateValidationException()
        {
            var list = new List<ValidationResult>
                    {
                        new ValidationResult("asdf", new [] { "asdf" }),
                    };

            var e = DomainValidationException.Create(list);
            return e;
        }

        protected void ExpectToLogDebugMessage()
        {
            MockLogger.Setup(x => x.Debug(It.IsAny<Exception>()));
        }

        protected void ExpectToLogErrorMessage()
        {
            MockLogger.Setup(x => x.Error(It.IsAny<Exception>()));
        }
    }
}

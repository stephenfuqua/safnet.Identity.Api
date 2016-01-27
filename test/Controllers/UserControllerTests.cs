using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Models;
using FligthNode.Identity.Services.Controllers;
using log4net;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers
{
    public class UserControllerTests
    {
        public class Fixture : IDisposable
        {
            protected MockRepository Repository = new MockRepository(MockBehavior.Strict);
            protected Mock<IUserDomainManager> MockManager;
            protected Mock<ILog> MockLogger;
            protected const string baseLocation = "http://localhost/v1/users";

            protected Fixture()
            {
                MockManager = Repository.Create<IUserDomainManager>();
                MockLogger = Repository.Create<ILog>();
            }

            protected UsersController BuildSystem()
            {
                var controller = new UsersController(MockManager.Object);
                controller.Logger = MockLogger.Object;

                controller.Request = new HttpRequestMessage() { RequestUri = new Uri(baseLocation) };
                controller.Configuration = new HttpConfiguration();

                return controller;
            }

            public void Dispose()
            {
                Repository.VerifyAll();
            }
        }

        public class ConstructorBehavior : Fixture
        {
            [Fact]
            public void ConfirmWithValidArgument()
            {
                Assert.NotNull(BuildSystem());
            }

            [Fact]
            public void ConfirmThatNullArgumentIsNotAllowed()
            {
                Assert.Throws<ArgumentNullException>(() => new UsersController(null));
            }
        }

        public class Register : Fixture
        {
            protected const string userName = "u33333";
            protected const string password = "asdfasdf999";
            protected const string firstName = "first";
            protected const string lastName = "dddss";
            protected const string phoneNumber = "5555555555";
            protected const string secondaryPhone = "1234567890";
            protected const string reporter = "Reporter";
            protected const string email = "email@email.com";
            protected const int userId = 3433;
            protected const string active = "pending";

            protected UserModel BuildInput()
            {
                return new UserModel
                {
                    Email = email,
                    FamilyName = lastName,
                    GivenName = firstName,
                    Password = password,
                    LockedOut = false, // this is how it will come in from clients
                    PrimaryPhoneNumber = phoneNumber,
                    Roles = new List<string>() { reporter },
                    SecondaryPhoneNumber = secondaryPhone,
                    UserName = userName
                };
            }

            [Fact]
            public void HappyPath()
            {
                //
                // Arrange
                var expectedLocation = (baseLocation + "/" + userId.ToString());

                MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                    .Callback((UserModel actual) =>
                    {
                        Assert.Equal(email, actual.Email);
                        Assert.Equal(lastName, actual.FamilyName);
                        Assert.Equal(firstName, actual.GivenName);
                        Assert.Equal(password, actual.Password);
                        Assert.Equal(true, actual.LockedOut);
                        Assert.Equal(phoneNumber, actual.PrimaryPhoneNumber);
                        Assert.Equal(secondaryPhone, actual.SecondaryPhoneNumber);
                        Assert.Equal(userName, actual.UserName);
                        Assert.Equal(active, actual.Active);
                    })
                    .Returns((UserModel response) =>
                    {
                        response.UserId = userId;
                        return response;
                    });

                //
                // Act
                var responseMessages = BuildSystem().Register(BuildInput()).ExecuteAsync(new System.Threading.CancellationToken()).Result;

                //
                // Assert
                Assert.Equal(expectedLocation, responseMessages.Headers.FirstOrDefault(x => x.Key == "Location").Value.First());
            }
        }

        public class GetAll
        {
            public class HappyPath : Fixture
            {
                private List<UserModel> list = new List<UserModel>
                {
                    new UserModel()
                };

                private HttpResponseMessage RunTest()
                {
                    MockManager.Setup(x => x.FindAll())
                        .Returns(list);
                    return BuildSystem().Get().ExecuteAsync(new System.Threading.CancellationToken()).Result;
                }

                [Fact]
                public void ConfirmHappyPathContent()
                {
                    Assert.Same(list.First(), RunTest().Content.ReadAsAsync<List<UserModel>>().Result.First());
                }

                [Fact]
                public void ConfirmHappyPathStatusCode()
                {
                    Assert.Equal(HttpStatusCode.OK, RunTest().StatusCode);
                }
            }

            public class NoRecords : Fixture
            {
                private HttpResponseMessage RunTest()
                {
                    MockManager.Setup(x => x.FindAll())
                        .Returns(new List<UserModel>());
                    return BuildSystem().Get().ExecuteAsync(new System.Threading.CancellationToken()).Result;
                }

                [Fact]
                public void ConfirmNotFoundStatusCode()
                {
                    Assert.Equal(HttpStatusCode.NotFound, RunTest().StatusCode);
                }
            }

            public class ExceptionHandling : Fixture
            {

                private HttpResponseMessage RunTest(Exception ex)
                {
                    MockManager.Setup(x => x.FindAll())
                        .Throws(ex);


                    return BuildSystem().Get().ExecuteAsync(new System.Threading.CancellationToken()).Result;
                }

                [Fact]
                public void ConfirmHandlingOfInvalidOperation()
                {
                    MockLogger.Setup(x => x.Error(It.IsAny<Exception>()));

                    var e = new InvalidOperationException();
                    Assert.Equal(HttpStatusCode.InternalServerError, RunTest(e).StatusCode);
                }

                [Fact]
                public void ConfirmHandlingOfServerError()
                {
                    MockLogger.Setup(x => x.Error(It.IsAny<Exception>()));

                    var e = ServerException.HandleException<ExceptionHandling>(new Exception(), "asdf");
                    Assert.Equal(HttpStatusCode.InternalServerError, RunTest(e).StatusCode);
                }

                [Fact]
                public void ConfirmHandlingOfUserError()
                {
                    MockLogger.Setup(x => x.Debug(It.IsAny<Exception>()));

                    var e = new UserException("asdf");
                    Assert.Equal(HttpStatusCode.BadRequest, RunTest(e).StatusCode);
                }
            }
        }
    }
}

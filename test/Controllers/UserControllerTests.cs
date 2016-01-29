using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Models;
using FligthNode.Identity.Services.Controllers;
using log4net;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Web.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers
{
    public class UserControllerTests
    {
        public class Fixture : IDisposable
        {
            public class UsersControllerTss : UsersController
            {
                public UsersControllerTss(IUserDomainManager manager) : base(manager) {
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

        public class Put : Fixture
        {
            private HttpResponseMessage RunTest(int id, UserModel input)
            {
                return BuildSystem().Put(id, input).ExecuteAsync(new System.Threading.CancellationToken()).Result;
            }


            public class HappyPath : Put
            {

                [Fact]
                public void ConfirmUsesInputIdNotModelId()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel
                    {
                        UserId = 99
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                        .Callback((UserModel actual) =>
                        {
                            Assert.Equal(id, actual.UserId);
                        });

                    //
                    // Act
                    RunTest(id, user);

                    // no additional asserts required
                }

                [Fact]
                public void ConfirmReturnsNoContent()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel
                    {
                        UserId = 99
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()));

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
                }


                [Fact]
                public void ConfirmPasswordUpdate()
                {
                    //
                    // Arrange
                    const int id = 33;
                    const string password = "new";
                    var user = new UserModel
                    {
                        UserId = 99,
                        Password = password
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()));
                    MockManager.Setup(x => x.AdministrativePasswordChange(It.Is<int>(y => y == id), It.Is<string>(y => y == password)));
                    
                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
                }
            }

            public class ErrorHandling : Put
            {
                [Fact]
                public void ConfirmHandlesUpdateError()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel();

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Asserts
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }

                [Fact]
                public void ConfirmDoesNotTryToSetPasswordAfterUpdateErrorOccurs()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel
                    {
                        Password = "asdfasd"
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    // nothing else to assert
                }


                [Fact]
                public void ConfirmHandlesErrorOnPasswordReset()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel
                    {
                        Password = "asdfasd"
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()));
                    
                    MockManager.Setup(x => x.AdministrativePasswordChange(It.Is<int>(y => y == id), It.IsAny<string>()))
                        .Throws(ServerException.HandleException<Exception>(new Exception(), "asdf"));

                    ExpectToLogErrorMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    // nothing else to assert
                }

                [Fact]
                public void ConfirmHandlingOfValidationErrors()
                {
                    //
                    // Arrange
                    const int id = 323;
                    const string email = "thisisnotvalid@something";
                    var user = new UserModel
                    {
                        Email = email
                    };

                    
                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                                               .Throws(CreateValidationException());

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }
            }
        }

        public class Profile : Fixture
        {
            protected const int LOGGED_IN_USER_ID = -999;

            private HttpResponseMessage RunTest(int id, UserModel input)
            {
                var principal = base.Repository.Create<IPrincipal>();
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, LOGGED_IN_USER_ID.ToString()));
                principal.SetupGet(x => x.Identity)
                    .Returns(identity);

                var system = BuildSystem();
                system.User = principal.Object;
                
                return system.Profile(id, input).ExecuteAsync(new System.Threading.CancellationToken()).Result;
            }


            public class HappyPath : Profile
            {

                [Fact]
                public void ConfirmUsesInputIdNotModelId()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    var user = new UserModel
                    {
                        UserId = 99
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                        .Callback((UserModel actual) =>
                        {
                            Assert.Equal(id, actual.UserId);
                        });

                    //
                    // Act
                    RunTest(id, user);

                    // no additional asserts required
                }

                [Fact]
                public void ConfirmReturnsNoContent()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    var user = new UserModel
                    {
                        UserId = 99
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()));

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
                }


                [Fact]
                public void ConfirmPasswordUpdate()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    const string password = "new";
                    var user = new UserModel
                    {
                        UserId = 99,
                        Password = password
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()));
                    MockManager.Setup(x => x.AdministrativePasswordChange(It.Is<int>(y => y == id), It.Is<string>(y => y == password)));

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
                }
            }

            public class ErrorHandling : Profile
            {
                [Fact]
                public void ConfirmHandlesUpdateError()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    var user = new UserModel();

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Asserts
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }

                [Fact]
                public void ConfirmDoesNotTryToSetPasswordAfterUpdateErrorOccurs()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    var user = new UserModel
                    {
                        Password = "asdfasd"
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    // nothing else to assert
                }


                [Fact]
                public void ConfirmHandlesErrorOnPasswordReset()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    var user = new UserModel
                    {
                        Password = "asdfasd"
                    };

                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()));

                    MockManager.Setup(x => x.AdministrativePasswordChange(It.Is<int>(y => y == id), It.IsAny<string>()))
                        .Throws(ServerException.HandleException<Exception>(new Exception(), "asdf"));

                    ExpectToLogErrorMessage();
                   
                    //
                    // Act
                    var result = RunTest(id, user);

                    // nothing else to assert
                }

                [Fact]
                public void ConfirmHandlingOfValidationErrors()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID;
                    const string email = "thisisnotvalid@something";
                    var user = new UserModel
                    {
                        Email = email
                    };


                    MockManager.Setup(x => x.Update(It.IsAny<UserModel>()))
                                               .Throws(CreateValidationException());

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }
                [Fact]
                public void ConfirmDoesNotAllowManagingSomeoneElsesProfiel()
                {
                    //
                    // Arrange
                    const int id = LOGGED_IN_USER_ID + 1; // Hey, the user managed to send the wrong ID!
                    var user = new UserModel();                    

                    //
                    // Act
                    var result = RunTest(id, user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }
            }
        }

        public class Register : Fixture
        {
            private HttpResponseMessage RunTest(UserModel input)
            {
                return BuildSystem().Register(input).ExecuteAsync(new System.Threading.CancellationToken()).Result;
            }


            public class HappyPath : Register
            {
                [Fact]
                public void ConfirmOverridesRoleToReporterOnly()
                {
                    //
                    // Arrange
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Callback((UserModel actual) =>
                        {
                            Assert.Equal(1, actual.Roles.Count());
                            Assert.Equal("Reporter", actual.Roles[0]);
                        })
                        .Returns(user);

                    //
                    // Act
                    RunTest(user);

                    // no additional asserts required
                }

                [Fact]
                public void ConfirmSetsLockedOutTrue()
                {
                    //
                    // Arrange
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Callback((UserModel actual) =>
                        {
                            Assert.True(user.LockedOut);
                        })
                        .Returns(user);

                    //
                    // Act
                    RunTest(user);

                    // no additional asserts required
                }

                [Fact]
                public void ConfirmSetsActiveToPending()
                {
                    //
                    // Arrange
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Callback((UserModel actual) =>
                        {
                            Assert.Equal("pending", actual.Active);
                        })
                        .Returns(user);

                    //
                    // Act
                    RunTest(user);

                    // no additional asserts required
                }

                [Fact]
                public void ConfirmReturnsCreated()
                {
                    //
                    // Arrange
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Returns(user);

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                }


                [Fact]
                public void ConfirmSetsLocationHeader()
                {
                    //
                    // Arrange
                    var id = 234234;
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Returns((UserModel modified) =>
                        {
                            modified.UserId = id;
                            return modified;
                        });

                    const string uri = "http://localhost";
                    const string expected = "http://localhost/";

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                }
            }

            public class ErrorHandling : Register
            {
                [Fact]
                public void ConfirmHandlesUpdateError()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Asserts
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }

                [Fact]
                public void ConfirmDoesNotTryToSetPasswordAfterUpdateErrorOccurs()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel
                    {
                        Password = "asdfasd"
                    };

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(user);

                    // nothing else to assert
                }



                [Fact]
                public void ConfirmHandlingOfValidationErrors()
                {
                    //
                    // Arrange
                    const int id = 323;
                    const string email = "thisisnotvalid@something";
                    var user = new UserModel
                    {
                        Email = email
                    };


                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                                               .Throws(CreateValidationException());

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }
            }
        }

        public class Post : Fixture
        {
            private HttpResponseMessage RunTest(UserModel input)
            {
                return BuildSystem().Post(input).ExecuteAsync(new System.Threading.CancellationToken()).Result;
            }


            public class HappyPath : Post
            {                
                [Fact]
                public void ConfirmReturnsCreated()
                {
                    //
                    // Arrange
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Returns(user);

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                }


                [Fact]
                public void ConfirmSetsLocationHeader()
                {
                    //
                    // Arrange
                    var id = 234234;
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Returns((UserModel modified) =>
                        {
                            modified.UserId = id;
                            return modified;
                        });

                    const string uri = "http://localhost";
                    const string expected = "http://localhost/";

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.Created, result.StatusCode);
                }
            }

            public class ErrorHandling : Post
            {
                [Fact]
                public void ConfirmHandlesUpdateError()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel();

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Asserts
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }

                [Fact]
                public void ConfirmDoesNotTryToSetPasswordAfterUpdateErrorOccurs()
                {
                    //
                    // Arrange
                    const int id = 33;
                    var user = new UserModel
                    {
                        Password = "asdfasd"
                    };

                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                        .Throws(new UserException("message"));

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(user);

                    // nothing else to assert
                }



                [Fact]
                public void ConfirmHandlingOfValidationErrors()
                {
                    //
                    // Arrange
                    const int id = 323;
                    const string email = "thisisnotvalid@something";
                    var user = new UserModel
                    {
                        Email = email
                    };


                    MockManager.Setup(x => x.Create(It.IsAny<UserModel>()))
                                               .Throws(CreateValidationException());

                    ExpectToLogDebugMessage();

                    //
                    // Act
                    var result = RunTest(user);

                    //
                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
                }
            }
        }
    }
}

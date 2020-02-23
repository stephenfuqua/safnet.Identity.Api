using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Controllers;
using FlightNode.Identity.Services.Models;
using log4net;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers
{
    public class RoleControllerTests
    {
        //[Fact]
        //public void GenerateNewAudienceId()
        //{
        //    var audience = JwtFormat.AddAudience("TernProd");
        //}

        public class Fixture : IDisposable
        {
            protected MockRepository Repository = new MockRepository(MockBehavior.Strict);
            protected Mock<IRoleManager> MockManager;
            protected Mock<ILog> MockLogger;

            protected Fixture()
            {
                MockManager = Repository.Create<IRoleManager>();
                MockLogger = Repository.Create<ILog>();
            }

            protected RolesController BuildSystem()
            {
                var controller = new RolesController(MockManager.Object);
                controller.Logger = MockLogger.Object;

                controller.Request = new HttpRequestMessage();
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
                Assert.Throws<ArgumentNullException>(() => new RolesController(null));
            }
        }

        public class GetAllRoles
        {
            public class HappyPath : Fixture
            {
                private List<RoleModel> list = new List<RoleModel>
                {
                    new RoleModel()
                };

                private OkNegotiatedContentResult<List<RoleModel>> RunTest()
                {
                    MockManager.Setup(x => x.FindAll())
                        .Returns(list);

                    var result = BuildSystem().Get();
                    return result as OkNegotiatedContentResult<List<RoleModel>>;
                }

                [Fact]
                public void ConfirmHappyPathContent()
                {
                    Assert.Same(list.First(), RunTest().Content.First());
                }

                [Fact]
                public void ConfirmHappyPathStatusCode()
                {
                    Assert.NotNull(RunTest());
                }
            
                [Fact]
                public void ConfirmNotFoundStatusCode()
                {
                    // Arrange mocks

                    MockManager.Setup(x => x.FindAll())
                        .Returns(new List<RoleModel>());

                    // Act
                    var result = BuildSystem().Get();

                    // Assert
                    Assert.IsType<NotFoundResult>(result);
                }
            }

            public class ExceptionHandling : Fixture
            {

                private void RunTest(Exception ex)
                {
                    MockManager.Setup(x => x.FindAll())
                        .Throws(ex);


                    var result = BuildSystem().Get();

                    Assert.IsType<InternalServerErrorResult>(result);
                }

                [Fact]
                public void ConfirmHandlingOfInvalidOperation()
                {
                    MockLogger.Setup(x => x.Error(It.IsAny<Exception>()));

                    var e = new InvalidOperationException();

                    RunTest(e);
                }

                [Fact]
                public void ConfirmHandlingOfServerError()
                {
                    MockLogger.Setup(x => x.Error(It.IsAny<Exception>()));

                    var e = ServerException.HandleException<ExceptionHandling>(new Exception(), "asdf");

                    RunTest(e);
                }

                [Fact]
                public void ConfirmHandlingOfUserError()
                {
                    MockLogger.Setup(x => x.Debug(It.IsAny<Exception>()));

                    var e = new UserException("asdf");

                    MockManager.Setup(x => x.FindAll())
                        .Throws(e);


                    var result = BuildSystem().Get();

                    Assert.IsType<BadRequestErrorMessageResult>(result);
                }
            }
        }
    }
}

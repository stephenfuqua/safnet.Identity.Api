﻿using FlightNode.Common.Exceptions;
using FlightNode.Identity.Domain.Interfaces;
using FlightNode.Identity.Services.Controllers;
using FlightNode.Identity.Services.Models;
using FlightNode.Identity.Services.Providers;
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

                private HttpResponseMessage RunTest()
                {
                    MockManager.Setup(x => x.FindAll())
                        .Returns(list);
                    return BuildSystem().Get().ExecuteAsync(new System.Threading.CancellationToken()).Result;
                }

                [Fact]
                public void ConfirmHappyPathContent()
                {
                    Assert.Same(list.First(), RunTest().Content.ReadAsAsync<List<RoleModel>>().Result.First());
                }

                [Fact]
                public void ConfirmHappyPathStatusCode()
                {
                    Assert.Equal(HttpStatusCode.OK, RunTest().StatusCode);
                }
            }

            public class NoRecords :Fixture
            {
                private HttpResponseMessage RunTest()
                {
                    MockManager.Setup(x => x.FindAll())
                        .Returns(new List<RoleModel>());
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

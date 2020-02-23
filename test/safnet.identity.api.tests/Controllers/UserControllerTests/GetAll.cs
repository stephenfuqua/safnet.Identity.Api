using FlightNode.Common.Exceptions;
using FlightNode.Identity.Services.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{

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

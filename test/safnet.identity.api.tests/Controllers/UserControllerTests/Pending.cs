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


    public class Pending : Fixture
    {
        protected HttpResponseMessage RunTest(List<PendingUserModel> expectedResult)
        {
            MockManager.Setup(x => x.FindAllPending())
                .Returns(expectedResult);
            return BuildSystem().Pending().ExecuteAsync(new System.Threading.CancellationToken()).Result;
        }

        public class HappyPath : Pending
        {
            private List<PendingUserModel> list = new List<PendingUserModel>
                {
                    new PendingUserModel()
                };


            [Fact]
            public void ConfirmHappyPathContent()
            {
                Assert.Same(list.First(), RunTest(list).Content.ReadAsAsync<List<PendingUserModel>>().Result.First());
            }

            [Fact]
            public void ConfirmHappyPathStatusCode()
            {
                Assert.Equal(HttpStatusCode.OK, RunTest(list).StatusCode);
            }
        }

        public class NoRecords : Pending
        {

            [Fact]
            public void ConfirmReturnsOk()
            {
                Assert.Equal(HttpStatusCode.OK, RunTest(new List<PendingUserModel>()).StatusCode);
            }


            [Fact]
            public void EvenNullReturnsOk()
            {
                Assert.Equal(HttpStatusCode.OK, RunTest(null as List<PendingUserModel>).StatusCode);
            }
        }

        public class ExceptionHandling : Fixture
        {

            private HttpResponseMessage RunTest(Exception ex)
            {
                MockManager.Setup(x => x.FindAllPending())
                    .Throws(ex);


                return BuildSystem().Pending().ExecuteAsync(new System.Threading.CancellationToken()).Result;
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

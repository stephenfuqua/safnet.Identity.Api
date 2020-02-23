using FlightNode.Common.Exceptions;
using FlightNode.Identity.Services.Models;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{
    public class Get : Fixture
    {
        private const int USER_ID = 34323;
        private UserModel user = new UserModel() { UserId = USER_ID };

        protected HttpResponseMessage RunTest(UserModel expected)
        {
            MockManager.Setup(x => x.FindById(It.Is<int>(y => y == USER_ID)))
                .Returns(expected);

            return BuildSystem().Get(USER_ID).ExecuteAsync(new System.Threading.CancellationToken()).Result;
        }

        public class HappyPath : Get
        {
            [Fact]
            public void ConfirmHappyPathContent()
            {
                Assert.Same(user, RunTest(user).Content.ReadAsAsync<UserModel>().Result);
            }

            [Fact]
            public void ConfirmHappyPathStatusCode()
            {
                Assert.Equal(HttpStatusCode.OK, RunTest(user).StatusCode);
            }
        }

        public class NoRecords : Get
        {


            [Fact]
            public void ConfirmNotFoundStatusCode()
            {
                Assert.Equal(HttpStatusCode.NotFound, RunTest(null).StatusCode);
            }
        }


        public class ExceptionHandling : Get
        {

            private HttpResponseMessage RunTest(Exception ex)
            {
                MockManager.Setup(x => x.FindById(It.IsAny<int>()))
                    .Throws(ex);


                return BuildSystem().Get(0).ExecuteAsync(new System.Threading.CancellationToken()).Result;
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

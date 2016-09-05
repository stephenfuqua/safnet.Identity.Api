using FlightNode.Common.Exceptions;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{
    public class Approve : Fixture
    {
        protected StatusCodeResult RunTest(List<int> ids)
        {
            MockManager.Setup(x => x.Approve(It.IsAny<List<int>>()))
                .Callback((List<int> actual) =>
                {
                    Assert.Same(ids, actual);
                })
                .Returns(Task.Delay(1));

            return BuildSystem().Approve(ids).Result as StatusCodeResult;
        }

        public class HappyPath : Approve
        {
            [Fact]
            public void ConfirmReturnsNoContent()
            {
                //
                // Arrange
                var ids = new List<int>() { 1, 2 };

                //
                // Act
                var result = RunTest(ids);

                //
                // Assert
                Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            }
        }

        public class ErrorHandling : Approve
        {

            private HttpResponseMessage RunTest(Exception ex)
            {
                MockManager.Setup(x => x.Approve(It.IsAny<List<int>>()))
                        .Throws(ex);
                return BuildSystem().Approve(new List<int>()).Result.ExecuteAsync(new System.Threading.CancellationToken()).Result;
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

                var e = ServerException.HandleException<ErrorHandling>(new Exception(), "asdf");
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

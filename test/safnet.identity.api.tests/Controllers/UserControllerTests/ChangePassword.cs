using FlightNode.Common.Exceptions;
using FlightNode.Identity.Services.Models;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{


    public class ChangePassword : Fixture
    {
        protected const int USER_ID = 234;
        protected const string OLD = "old";
        protected const string NEW = "new";


        protected HttpResponseMessage RunTest()
        {
            return BuildSystem().ChangePassword(USER_ID, new PasswordModel { CurrentPassword = OLD, NewPassword = NEW }).ExecuteAsync(new System.Threading.CancellationToken()).Result;
        }

        public class HappyPath : ChangePassword
        {


            [Fact]
            public void ConfirmReturnsNoContent()
            {
                //
                // Arrange
                MockManager.Setup(x => x.ChangePassword(It.IsAny<int>(), It.IsAny<PasswordModel>()));

                //
                // Act
                var result = RunTest();

                //
                // Assert
                Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
            }

            [Fact]
            public void ConfirmUserId()
            {
                //
                // Arrange
                MockManager.Setup(x => x.ChangePassword(It.Is<int>(y => y == USER_ID), It.IsAny<PasswordModel>()));

                //
                // Act
                var result = RunTest();

                // no assertions required
            }


            [Fact]
            public void ConfirmOldPassword()
            {
                //
                // Arrange
                MockManager.Setup(x => x.ChangePassword(It.IsAny<int>(), It.Is<PasswordModel>(y => y.CurrentPassword == OLD)));

                //
                // Act
                var result = RunTest();

                // no assertions required
            }


            [Fact]
            public void ConfirmNewdPassword()
            {
                //
                // Arrange
                MockManager.Setup(x => x.ChangePassword(It.IsAny<int>(), It.Is<PasswordModel>(y => y.NewPassword == NEW)));

                //
                // Act
                var result = RunTest();

                // no assertions required
            }

        }

        public class ErrorHandling : ChangePassword
        {

            private HttpResponseMessage RunTest(Exception ex)
            {
                MockManager.Setup(x => x.ChangePassword(It.IsAny<int>(), It.IsAny<PasswordModel>()))
                        .Throws(ex);
                return BuildSystem().ChangePassword(USER_ID, new PasswordModel { CurrentPassword = OLD, NewPassword = NEW }).ExecuteAsync(new System.Threading.CancellationToken()).Result;
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

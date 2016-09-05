using FlightNode.Common.Exceptions;
using FlightNode.Identity.Services.Models;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{


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
}

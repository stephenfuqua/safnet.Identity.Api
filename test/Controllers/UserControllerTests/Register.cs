using FlightNode.Common.Exceptions;
using FlightNode.Identity.Services.Models;
using Moq;
using System.Net;
using System.Net.Http;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{

    public class Register : Fixture
    {
        private HttpResponseMessage RunTest(UserModel input)
        {
            return BuildSystem().Register(input).ExecuteAsync(new System.Threading.CancellationToken()).Result;
        }


        public class HappyPath : Register
        {
            [Fact]
            public void ConfirmReturnsCreated()
            {
                //
                // Arrange
                var user = new UserModel();

                MockManager.Setup(x => x.CreatePending(It.IsAny<UserModel>()))
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

                MockManager.Setup(x => x.CreatePending(It.IsAny<UserModel>()))
                    .Returns((UserModel modified) =>
                    {
                        modified.UserId = id;
                        return modified;
                    });


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
                var user = new UserModel();

                MockManager.Setup(x => x.CreatePending(It.IsAny<UserModel>()))
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
                var user = new UserModel
                {
                    Password = "asdfasd"
                };


                MockManager.Setup(x => x.CreatePending(It.IsAny<UserModel>()))
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
                const string email = "thisisnotvalid@something";
                var user = new UserModel
                {
                    Email = email
                };


                MockManager.Setup(x => x.CreatePending(It.IsAny<UserModel>()))
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

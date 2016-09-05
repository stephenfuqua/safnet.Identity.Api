
using FlightNode.Identity.Domain.Entities;
using FlightNode.Identity.Services.Models;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{
    public class ChangeOwnPassword : Fixture
    {
        [Fact]
        public void EmptyTokenResultsInBadRequest()
        {
            const string token = "  ";
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "b" };

            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            Assert.IsType<BadRequestErrorMessageResult>(result);
        }

        [Fact]
        public void NullTokenResultsInBadRequest()
        {
            const string token = null;
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "b" };

            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            Assert.IsType<BadRequestErrorMessageResult>(result);
        }


        [Fact]
        public void NullInputModelResultsInBadRequest()
        {
            const string token = "asdf";

            var result = BuildSystem().ChangeOwnPassword(token, null).Result;

            Assert.IsType<InvalidModelStateResult>(result);
        }

        [Fact]
        public void MissingEmailInInputResultsInBadRequest()
        {
            const string token = "asdf";
            var input = new ChangePasswordModel { EmailAddress = "", Password = "b" };

            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            Assert.IsType<InvalidModelStateResult>(result);
        }

        [Fact]
        public void MissingPasswordInInputResultsInBadRequest()
        {
            const string token = "asdf";
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "" };

            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            Assert.IsType<InvalidModelStateResult>(result);
        }


        [Fact]
        public void ValidInputChangesThePasswordAndReturnsOk()
        {
            // Arrange
            const string token = "asdf";
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "asdfasd" };


            MockManager.Setup(x => x.ChangeForgottenPassword(token, input))
                .Returns(Task.FromResult(ChangeForgottenPasswordResult.Happy));

            // Act
            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public void BadEmailReturns404()
        {
            // Arrange
            const string token = "asdf";
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "ddd" };


            MockManager.Setup(x => x.ChangeForgottenPassword(token, input))
                .Returns(Task.FromResult(ChangeForgottenPasswordResult.UserDoesNotExist));

            // Act
            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            // Assert
            Assert.IsType<NotFoundResult>(result);
            //Assert.Equal((HttpStatusCode)422, (result as StatusCodeResult).StatusCode);
        }

        [Fact]
        public void BadTokenReturns422()
        {
            // Arrange
            const string token = "asdf";
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "ddd" };


            MockManager.Setup(x => x.ChangeForgottenPassword(token, input))
                .Returns(Task.FromResult(ChangeForgottenPasswordResult.BadToken));

            // Act
            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal((HttpStatusCode)422, (result as StatusCodeResult).StatusCode);
        }
        

        [Fact]
        public void PasswordIsInsufficientlyComplextReturns400()
        {
            // Arrange
            const string token = "asdf";
            var input = new ChangePasswordModel { EmailAddress = "a", Password = "ddd" };


            MockManager.Setup(x => x.ChangeForgottenPassword(token, input))
                .Returns(Task.FromResult(ChangeForgottenPasswordResult.InvalidPassword));

            // Act
            var result = BuildSystem().ChangeOwnPassword(token, input).Result;

            // Assert
            Assert.IsType<InvalidModelStateResult>(result);
        }
    }
}

using FlightNode.Identity.Services.Models;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{

    public class RequestPasswordChange : Fixture
    {
        
        [Fact]
        public void GivenNullEmailAddressThenReturnBadRequest()
        {
            var result = BuildSystem().RequestPasswordChange(null).Result;

            Assert.IsType<InvalidModelStateResult>(result);
        }

        [Fact]
        public void GivenEmptyEmailAddressThenReturnBadRequest()
        {
            var result = BuildSystem().RequestPasswordChange(new RequestPasswordResetModel { EmailAddress = "    " }).Result;

            Assert.IsType<InvalidModelStateResult>(result);
        }

        [Fact]
        public void GivenEmailThatDoesntExistThenReturnUnprocessable()
        {
            // Arrange
            var input = new RequestPasswordResetModel { EmailAddress = "dddd" };

            MockManager.Setup(x => x.RequestPasswordChange(input.EmailAddress))
                .Returns(Task.FromResult(false));

            // Act
            var result = BuildSystem().RequestPasswordChange(input).Result;

            // Assert
            Assert.IsType<StatusCodeResult>(result);
            Assert.Equal(422, (int)(result as StatusCodeResult).StatusCode);
        }

        [Fact]
        public void GivenEmailThatExistsThenReturnOk()
        {
            // Arrange
            var input = new RequestPasswordResetModel { EmailAddress = "dddd" };

            MockManager.Setup(x => x.RequestPasswordChange(input.EmailAddress))
                .Returns(Task.FromResult(true));

            // Act
            var result = BuildSystem().RequestPasswordChange(input).Result;

            // Assert
            Assert.IsType<OkResult>(result);
        }
    }

}

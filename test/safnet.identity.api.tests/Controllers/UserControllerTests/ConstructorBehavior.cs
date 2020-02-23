using FligthNode.Identity.Services.Controllers;
using System;
using Xunit;

namespace FlightNode.Identity.UnitTests.Controllers.UserControllerTests
{
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
            Assert.Throws<ArgumentNullException>(() => new UsersController(null));
        }
    }
    
}